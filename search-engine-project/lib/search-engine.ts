import type { Document, SearchResult, ParsedQuery } from "@/types/search-types"
import { DocumentIndexer } from "./document-indexer"
import { QueryParser } from "./query-parser"
import { RankingAlgorithm } from "./ranking-algorithm"

/**
 * Main Search Engine class implementing the Abstract Data Type (ADT)
 * This class orchestrates the entire search process
 */
export class SearchEngine {
  private indexer: DocumentIndexer
  private queryParser: QueryParser
  private rankingAlgorithm: RankingAlgorithm
  private documents: Map<string, Document>

  constructor() {
    this.indexer = new DocumentIndexer()
    this.queryParser = new QueryParser()
    this.rankingAlgorithm = new RankingAlgorithm()
    this.documents = new Map()
  }

  /**
   * Index a document for searching
   * @param document Document to be indexed
   */
  async indexDocument(document: Document): Promise<void> {
    // Store the document
    this.documents.set(document.id, document)

    // Index the document content
    await this.indexer.indexDocument(document)

    // Update ranking algorithm with new document
    this.rankingAlgorithm.updateDocumentStats(document.id, document.content.length)
  }

  /**
   * Search for documents matching the query
   * @param query Search query string
   * @returns Array of search results ranked by relevance
   */
  async search(query: string): Promise<SearchResult[]> {
    const startTime = performance.now()

    // Parse the query
    const parsedQuery = this.queryParser.parseQuery(query)

    // Get candidate documents from inverted index
    const candidateDocuments = this.getCandidateDocuments(parsedQuery)

    // Calculate relevance scores
    const scoredResults = this.rankingAlgorithm.calculateScores(
      candidateDocuments,
      parsedQuery,
      this.indexer.getInvertedIndex(),
      this.documents.size,
    )

    // Convert to search results with snippets
    const searchResults = this.createSearchResults(scoredResults, parsedQuery)

    const endTime = performance.now()
    console.log(`Search completed in ${(endTime - startTime).toFixed(2)}ms`)

    return searchResults
  }

  /**
   * Get auto-complete suggestions for a partial query
   * @param partialQuery Partial query string
   * @returns Array of suggested completions
   */
  getAutoComplete(partialQuery: string): string[] {
    return this.indexer.getAutoCompleteSuggestions(partialQuery.toLowerCase(), 5)
  }

  /**
   * Get the size of the current index
   * @returns Number of unique terms in the index
   */
  getIndexSize(): number {
    return this.indexer.getIndexSize()
  }

  /**
   * Get candidate documents that contain query terms
   */
  private getCandidateDocuments(parsedQuery: ParsedQuery): Set<string> {
    const candidates = new Set<string>()
    const invertedIndex = this.indexer.getInvertedIndex()

    for (const queryTerm of parsedQuery.terms) {
      const indexEntry = invertedIndex.get(queryTerm.term)
      if (indexEntry) {
        for (const posting of indexEntry.postings) {
          if (queryTerm.isExcluded) {
            candidates.delete(posting.documentId)
          } else {
            candidates.add(posting.documentId)
          }
        }
      }
    }

    return candidates
  }

  /**
   * Create search result objects with snippets
   */
  private createSearchResults(
    scoredResults: Array<{ documentId: string; score: number; matchedTerms: string[] }>,
    parsedQuery: ParsedQuery,
  ): SearchResult[] {
    return scoredResults.map((result) => {
      const document = this.documents.get(result.documentId)!
      const snippet = this.generateSnippet(document.content, result.matchedTerms)

      return {
        id: document.id,
        title: document.title,
        content: document.content,
        snippet,
        score: result.score,
        type: document.type,
        size: document.size,
        matchedTerms: result.matchedTerms,
      }
    })
  }

  /**
   * Generate a snippet showing matched terms in context
   */
  private generateSnippet(content: string, matchedTerms: string[]): string {
    const words = content.toLowerCase().split(/\s+/)
    const snippetLength = 150

    // Find the first occurrence of any matched term
    let startIndex = 0
    for (let i = 0; i < words.length; i++) {
      if (matchedTerms.some((term) => words[i].includes(term.toLowerCase()))) {
        startIndex = Math.max(0, i - 10)
        break
      }
    }

    const endIndex = Math.min(words.length, startIndex + 30)
    const snippetWords = content.split(/\s+/).slice(startIndex, endIndex)
    let snippet = snippetWords.join(" ")

    if (snippet.length > snippetLength) {
      snippet = snippet.substring(0, snippetLength) + "..."
    }

    return snippet
  }
}
