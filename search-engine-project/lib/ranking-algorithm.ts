import type { IndexEntry, ParsedQuery } from "@/types/search-types"

/**
 * Ranking Algorithm class implementing TF-IDF scoring
 * Calculates document relevance scores based on term frequency and inverse document frequency
 */
export class RankingAlgorithm {
  private documentLengths: Map<string, number>
  private totalDocuments: number

  constructor() {
    this.documentLengths = new Map()
    this.totalDocuments = 0
  }

  /**
   * Update document statistics for ranking calculations
   * @param documentId Document ID
   * @param length Document length in characters
   */
  updateDocumentStats(documentId: string, length: number): void {
    if (!this.documentLengths.has(documentId)) {
      this.totalDocuments++
    }
    this.documentLengths.set(documentId, length)
  }

  /**
   * Calculate relevance scores for candidate documents
   * @param candidateDocuments Set of candidate document IDs
   * @param parsedQuery Parsed search query
   * @param invertedIndex Inverted index for term lookups
   * @param totalDocuments Total number of documents in collection
   * @returns Array of scored results sorted by relevance
   */
  calculateScores(
    candidateDocuments: Set<string>,
    parsedQuery: ParsedQuery,
    invertedIndex: Map<string, IndexEntry>,
    totalDocuments: number,
  ): Array<{ documentId: string; score: number; matchedTerms: string[] }> {
    const scoredResults: Array<{ documentId: string; score: number; matchedTerms: string[] }> = []

    for (const documentId of candidateDocuments) {
      const score = this.calculateDocumentScore(documentId, parsedQuery, invertedIndex, totalDocuments)
      const matchedTerms = this.getMatchedTerms(documentId, parsedQuery, invertedIndex)

      if (score > 0) {
        scoredResults.push({
          documentId,
          score,
          matchedTerms,
        })
      }
    }

    // Sort by score in descending order
    return scoredResults.sort((a, b) => b.score - a.score)
  }

  /**
   * Calculate TF-IDF score for a document given a query
   * @param documentId Document ID
   * @param parsedQuery Parsed query
   * @param invertedIndex Inverted index
   * @param totalDocuments Total number of documents
   * @returns Relevance score
   */
  private calculateDocumentScore(
    documentId: string,
    parsedQuery: ParsedQuery,
    invertedIndex: Map<string, IndexEntry>,
    totalDocuments: number,
  ): number {
    let totalScore = 0
    let matchedTerms = 0

    for (const queryTerm of parsedQuery.terms) {
      const indexEntry = invertedIndex.get(queryTerm.term)

      if (!indexEntry) continue

      const posting = indexEntry.postings.find((p) => p.documentId === documentId)

      if (!posting) {
        // If this is a required term and it's not found, return 0
        if (queryTerm.isRequired) {
          return 0
        }
        continue
      }

      // If this is an excluded term and it's found, return 0
      if (queryTerm.isExcluded) {
        return 0
      }

      // Calculate TF-IDF score
      const tf = this.calculateTF(posting.termFrequency, this.documentLengths.get(documentId) || 1)
      const idf = this.calculateIDF(indexEntry.documentFrequency, totalDocuments)
      const termScore = tf * idf * queryTerm.weight

      totalScore += termScore
      matchedTerms++
    }

    // Apply query coverage bonus
    const queryCoverage = matchedTerms / parsedQuery.terms.length
    const coverageBonus = queryCoverage * 0.1

    return totalScore + coverageBonus
  }

  /**
   * Calculate Term Frequency (TF) using log normalization
   * @param termFrequency Raw term frequency
   * @param documentLength Document length
   * @returns Normalized TF score
   */
  private calculateTF(termFrequency: number, documentLength: number): number {
    // Log normalization with document length consideration
    const normalizedTF = termFrequency / Math.sqrt(documentLength)
    return 1 + Math.log(normalizedTF)
  }

  /**
   * Calculate Inverse Document Frequency (IDF)
   * @param documentFrequency Number of documents containing the term
   * @param totalDocuments Total number of documents
   * @returns IDF score
   */
  private calculateIDF(documentFrequency: number, totalDocuments: number): number {
    if (documentFrequency === 0) return 0
    return Math.log(totalDocuments / documentFrequency)
  }

  /**
   * Get matched terms for a document
   * @param documentId Document ID
   * @param parsedQuery Parsed query
   * @param invertedIndex Inverted index
   * @returns Array of matched terms
   */
  private getMatchedTerms(
    documentId: string,
    parsedQuery: ParsedQuery,
    invertedIndex: Map<string, IndexEntry>,
  ): string[] {
    const matchedTerms: string[] = []

    for (const queryTerm of parsedQuery.terms) {
      const indexEntry = invertedIndex.get(queryTerm.term)

      if (indexEntry && indexEntry.postings.some((p) => p.documentId === documentId)) {
        matchedTerms.push(queryTerm.term)
      }
    }

    return matchedTerms
  }
}
