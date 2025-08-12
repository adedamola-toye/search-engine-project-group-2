"use client"

import { useState } from "react"
import { SearchInterface } from "@/components/search-interface"
import { DocumentUpload } from "@/components/document-upload"
import { SearchResults } from "@/components/search-results"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { SearchEngine } from "@/lib/search-engine"
import type { SearchResult } from "@/types/search-types"

export default function HomePage() {
  const [searchEngine] = useState(() => new SearchEngine())
  const [searchResults, setSearchResults] = useState<SearchResult[]>([])
  const [isSearching, setIsSearching] = useState(false)
  const [indexedDocuments, setIndexedDocuments] = useState(0)

  const handleSearch = async (query: string) => {
    setIsSearching(true)
    try {
      const startTime = performance.now()
      const results = await searchEngine.search(query)
      const endTime = performance.now()

      console.log(`Search completed in ${(endTime - startTime).toFixed(2)}ms`)
      setSearchResults(results)
    } catch (error) {
      console.error("Search error:", error)
      setSearchResults([])
    } finally {
      setIsSearching(false)
    }
  }

  const handleDocumentIndexed = () => {
    setIndexedDocuments((prev) => prev + 1)
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-8">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">University Search Engine</h1>
          <p className="text-lg text-gray-600">Advanced document search with real-time indexing and ranking</p>
        </div>

        <Tabs defaultValue="search" className="w-full">
          <TabsList className="grid w-full grid-cols-2 mb-6">
            <TabsTrigger value="search">Search Documents</TabsTrigger>
            <TabsTrigger value="manage">Manage Documents</TabsTrigger>
          </TabsList>

          <TabsContent value="search" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Document Search</CardTitle>
                <CardDescription>Search through indexed documents with advanced ranking algorithms</CardDescription>
              </CardHeader>
              <CardContent>
                <SearchInterface onSearch={handleSearch} isSearching={isSearching} searchEngine={searchEngine} />
              </CardContent>
            </Card>

            {searchResults.length > 0 && <SearchResults results={searchResults} />}
          </TabsContent>

          <TabsContent value="manage" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Document Management</CardTitle>
                <CardDescription>
                  Upload and index documents for searching. Supports PDF, DOC, TXT, HTML, and XML files.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <DocumentUpload searchEngine={searchEngine} onDocumentIndexed={handleDocumentIndexed} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Index Statistics</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-4">
                  <div className="text-center p-4 bg-blue-50 rounded-lg">
                    <div className="text-2xl font-bold text-blue-600">{indexedDocuments}</div>
                    <div className="text-sm text-gray-600">Documents Indexed</div>
                  </div>
                  <div className="text-center p-4 bg-green-50 rounded-lg">
                    <div className="text-2xl font-bold text-green-600">{searchEngine.getIndexSize()}</div>
                    <div className="text-sm text-gray-600">Unique Terms</div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  )
}
