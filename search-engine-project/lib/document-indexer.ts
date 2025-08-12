import type { Document, IndexEntry } from "@/types/search-types"

/**
 * Document Indexer class responsible for creating and maintaining inverted indexes
 * Implements stop word filtering and term frequency calculation
 */
export class DocumentIndexer {
  private invertedIndex: Map<string, IndexEntry>
  private stopWords: Set<string>

  constructor() {
    this.invertedIndex = new Map()
    this.stopWords = new Set([
      "a",
      "an",
      "and",
      "are",
      "as",
      "at",
      "be",
      "by",
      "for",
      "from",
      "has",
      "he",
      "in",
      "is",
      "it",
      "its",
      "of",
      "on",
      "that",
      "the",
      "to",
      "was",
      "will",
      "with",
      "would",
      "could",
      "should",
      "may",
      "might",
      "must",
      "can",
      "shall",
      "this",
      "these",
      "those",
      "they",
      "them",
      "their",
      "there",
      "where",
      "when",
      "what",
      "who",
      "why",
      "how",
      "which",
      "while",
      "during",
      "before",
      "after",
      "above",
      "below",
      "up",
      "down",
      "out",
      "off",
      "over",
      "under",
      "again",
      "further",
      "then",
      "once",
    ])
  }

  /**
   * Index a document by extracting terms and building inverted index
   * @param document Document to be indexed
   */
  async indexDocument(document: Document): Promise<void> {
    const terms = this.extractTerms(document.content)
    const termFrequencies = this.calculateTermFrequencies(terms)

    // Update inverted index for each term
    for (const [term, frequency] of termFrequencies) {
      if (!this.stopWords.has(term) && term.length > 2) {
        this.updateInvertedIndex(term, document.id, frequency, this.getTermPositions(terms, term))
      }
    }
  }

  /**
   * Extract terms from document content
   * @param content Document content
   * @returns Array of normalized terms
   */
  private extractTerms(content: string): string[] {
    // Remove HTML tags, special characters, and normalize
    const cleanContent = content
      .replace(/<[^>]*>/g, " ") // Remove HTML tags
      .replace(/[^\w\s]/g, " ") // Remove special characters
      .toLowerCase()
      .trim()

    // Split into terms and filter empty strings
    return cleanContent.split(/\s+/).filter((term) => term.length > 0)
  }

  /**
   * Calculate term frequencies in the document
   * @param terms Array of terms
   * @returns Map of term to frequency
   */
  private calculateTermFrequencies(terms: string[]): Map<string, number> {
    const frequencies = new Map<string, number>()

    for (const term of terms) {
      frequencies.set(term, (frequencies.get(term) || 0) + 1)
    }

    return frequencies
  }

  /**
   * Get positions of a term in the document
   * @param terms Array of all terms
   * @param targetTerm Term to find positions for
   * @returns Array of positions
   */
  private getTermPositions(terms: string[], targetTerm: string): number[] {
    const positions: number[] = []

    for (let i = 0; i < terms.length; i++) {
      if (terms[i] === targetTerm) {
        positions.push(i)
      }
    }

    return positions
  }

  /**
   * Update the inverted index with term information
   * @param term The term to index
   * @param documentId Document ID
   * @param termFrequency Frequency of term in document
   * @param positions Positions of term in document
   */
  private updateInvertedIndex(term: string, documentId: string, termFrequency: number, positions: number[]): void {
    let indexEntry = this.invertedIndex.get(term)

    if (!indexEntry) {
      indexEntry = {
        term,
        documentFrequency: 0,
        postings: [],
      }
      this.invertedIndex.set(term, indexEntry)
    }

    // Check if document already exists in postings
    const existingPosting = indexEntry.postings.find((p) => p.documentId === documentId)

    if (existingPosting) {
      // Update existing posting
      existingPosting.termFrequency = termFrequency
      existingPosting.positions = positions
    } else {
      // Add new posting
      indexEntry.postings.push({
        documentId,
        termFrequency,
        positions,
      })
      indexEntry.documentFrequency++
    }
  }

  /**
   * Get auto-complete suggestions based on partial term
   * @param partialTerm Partial term to complete
   * @param maxSuggestions Maximum number of suggestions
   * @returns Array of suggested completions
   */
  getAutoCompleteSuggestions(partialTerm: string, maxSuggestions = 5): string[] {
    const suggestions: string[] = []

    for (const [term] of this.invertedIndex) {
      if (term.startsWith(partialTerm) && suggestions.length < maxSuggestions) {
        suggestions.push(term)
      }
    }

    return suggestions.sort()
  }

  /**
   * Get the inverted index
   * @returns The inverted index map
   */
  getInvertedIndex(): Map<string, IndexEntry> {
    return this.invertedIndex
  }

  /**
   * Get the size of the index
   * @returns Number of unique terms
   */
  getIndexSize(): number {
    return this.invertedIndex.size
  }
}
