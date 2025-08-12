import type { ParsedQuery, QueryTerm } from "@/types/search-types"

/**
 * Query Parser class responsible for parsing and processing search queries
 * Supports boolean operators, phrase queries, and query normalization
 */
export class QueryParser {
  private stopWords: Set<string>

  constructor() {
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
   * Parse a search query into structured query terms
   * @param query Raw search query
   * @returns Parsed query object
   */
  parseQuery(query: string): ParsedQuery {
    const originalQuery = query
    let processedQuery = query.toLowerCase().trim()

    // Handle phrase queries (quoted strings)
    const phraseMatches = processedQuery.match(/"([^"]+)"/g)
    const phrases: string[] = []

    if (phraseMatches) {
      for (const phrase of phraseMatches) {
        const cleanPhrase = phrase.replace(/"/g, "")
        phrases.push(cleanPhrase)
        processedQuery = processedQuery.replace(phrase, "")
      }
    }

    // Extract individual terms
    const terms = this.extractQueryTerms(processedQuery)

    // Add phrase terms
    for (const phrase of phrases) {
      const phraseTerms = phrase.split(/\s+/).filter((term) => term.length > 0 && !this.stopWords.has(term))
      for (const term of phraseTerms) {
        terms.push({
          term: term,
          weight: 2.0, // Phrases get higher weight
          isRequired: true,
          isExcluded: false,
        })
      }
    }

    return {
      terms,
      originalQuery,
      processedQuery: terms.map((t) => t.term).join(" "),
    }
  }

  /**
   * Extract query terms with boolean operators
   * @param query Processed query string
   * @returns Array of query terms
   */
  private extractQueryTerms(query: string): QueryTerm[] {
    const terms: QueryTerm[] = []
    const tokens = query.split(/\s+/).filter((token) => token.length > 0)

    for (let i = 0; i < tokens.length; i++) {
      const token = tokens[i]

      // Handle exclusion operator (-)
      if (token.startsWith("-") && token.length > 1) {
        const term = token.substring(1)
        if (!this.stopWords.has(term) && term.length > 2) {
          terms.push({
            term,
            weight: 1.0,
            isRequired: false,
            isExcluded: true,
          })
        }
        continue
      }

      // Handle required operator (+)
      if (token.startsWith("+") && token.length > 1) {
        const term = token.substring(1)
        if (!this.stopWords.has(term) && term.length > 2) {
          terms.push({
            term,
            weight: 1.5,
            isRequired: true,
            isExcluded: false,
          })
        }
        continue
      }

      // Handle boolean operators
      if (token === "and" || token === "or" || token === "not") {
        continue // Skip boolean operators for now
      }

      // Regular term
      if (!this.stopWords.has(token) && token.length > 2) {
        terms.push({
          term: token,
          weight: 1.0,
          isRequired: false,
          isExcluded: false,
        })
      }
    }

    return terms
  }

  /**
   * Normalize a term for consistent indexing and searching
   * @param term Raw term
   * @returns Normalized term
   */
  private normalizeTerm(term: string): string {
    return term.toLowerCase().replace(/[^\w]/g, "").trim()
  }
}
