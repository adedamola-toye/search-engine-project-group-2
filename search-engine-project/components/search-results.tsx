"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { ScrollArea } from "@/components/ui/scroll-area"
import { FileText, Star, Eye } from "lucide-react"
import type { SearchResult } from "@/types/search-types"

interface SearchResultsProps {
  results: SearchResult[]
}

export function SearchResults({ results }: SearchResultsProps) {
  const [selectedDocument, setSelectedDocument] = useState<SearchResult | null>(null)

  if (results.length === 0) {
    return (
      <Card>
        <CardContent className="text-center py-8">
          <FileText className="mx-auto h-12 w-12 text-gray-400 mb-4" />
          <p className="text-gray-500">No documents found matching your search criteria.</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Search Results ({results.length})
          </CardTitle>
          <CardDescription>Results ranked by relevance using TF-IDF scoring</CardDescription>
        </CardHeader>
      </Card>

      {results.map((result, index) => (
        <Card key={result.id} className="hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <h3 className="text-lg font-semibold text-blue-600">{result.title}</h3>
                  <Badge variant="secondary" className="text-xs">
                    Rank #{index + 1}
                  </Badge>
                </div>

                <p className="text-gray-600 mb-3 line-clamp-3">{result.snippet}</p>

                <div className="flex items-center gap-4 text-sm text-gray-500">
                  <div className="flex items-center gap-1">
                    <Star className="h-4 w-4" />
                    Score: {result.score.toFixed(4)}
                  </div>
                  <div>Type: {result.type}</div>
                  <div>Size: {result.size} chars</div>
                </div>
              </div>

              <Dialog>
                <DialogTrigger asChild>
                  <Button variant="outline" size="sm" className="ml-4 bg-transparent">
                    <Eye className="h-4 w-4 mr-2" />
                    View
                  </Button>
                </DialogTrigger>
                <DialogContent className="max-w-4xl max-h-[80vh]">
                  <DialogHeader>
                    <DialogTitle>{result.title}</DialogTitle>
                    <DialogDescription>Document content with highlighted search terms</DialogDescription>
                  </DialogHeader>
                  <ScrollArea className="h-[60vh] w-full rounded-md border p-4">
                    <div className="whitespace-pre-wrap text-sm">{result.content}</div>
                  </ScrollArea>
                </DialogContent>
              </Dialog>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
