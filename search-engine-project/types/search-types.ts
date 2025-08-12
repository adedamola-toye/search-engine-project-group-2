export interface Document {
  id: string
  title: string
  content: string
  type: string
  size: number
  lastModified: Date
}

export interface SearchResult {
  id: string
  title: string
  content: string
  snippet: string
  score: number
  type: string
  size: number
  matchedTerms: string[]
}

export interface IndexEntry {
  term: string
  documentFrequency: number
  postings: PostingEntry[]
}

export interface PostingEntry {
  documentId: string
  termFrequency: number
  positions: number[]
}

export interface QueryTerm {
  term: string
  weight: number
  isRequired: boolean
  isExcluded: boolean
}

export interface ParsedQuery {
  terms: QueryTerm[]
  originalQuery: string
  processedQuery: string
}
