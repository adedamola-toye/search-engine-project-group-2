"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Search, Clock } from "lucide-react"
import type { SearchEngine } from "@/lib/search-engine"

interface SearchInterfaceProps {
  onSearch: (query: string) => void
  isSearching: boolean
  searchEngine: SearchEngine
}

export function SearchInterface({ onSearch, isSearching, searchEngine }: SearchInterfaceProps) {
  const [query, setQuery] = useState("")
  const [suggestions, setSuggestions] = useState<string[]>([])
  const [showSuggestions, setShowSuggestions] = useState(false)

  useEffect(() => {
    if (query.length > 2) {
      const autoComplete = searchEngine.getAutoComplete(query)
      setSuggestions(autoComplete)
      setShowSuggestions(autoComplete.length > 0)
    } else {
      setSuggestions([])
      setShowSuggestions(false)
    }
  }, [query, searchEngine])

  const handleSearch = () => {
    if (query.trim()) {
      onSearch(query.trim())
      setShowSuggestions(false)
    }
  }

  const handleSuggestionClick = (suggestion: string) => {
    setQuery(suggestion)
    onSearch(suggestion)
    setShowSuggestions(false)
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch()
    }
  }

  return (
    <div className="space-y-4">
      <div className="relative">
        <div className="flex gap-2">
          <div className="relative flex-1">
            <Input
              type="text"
              placeholder="Enter your search query..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onKeyPress={handleKeyPress}
              className="pr-10"
            />
            <Search className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
          </div>
          <Button onClick={handleSearch} disabled={isSearching || !query.trim()} className="px-6">
            {isSearching ? (
              <>
                <Clock className="mr-2 h-4 w-4 animate-spin" />
                Searching...
              </>
            ) : (
              "Search"
            )}
          </Button>
        </div>

        {showSuggestions && (
          <div className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-md shadow-lg z-10 max-h-48 overflow-y-auto">
            {suggestions.map((suggestion, index) => (
              <button
                key={index}
                className="w-full text-left px-4 py-2 hover:bg-gray-50 border-b border-gray-100 last:border-b-0"
                onClick={() => handleSuggestionClick(suggestion)}
              >
                <span className="text-sm">{suggestion}</span>
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="flex flex-wrap gap-2">
        <Badge variant="outline" className="text-xs">
          Response Time: {"<"} 0.01s
        </Badge>
        <Badge variant="outline" className="text-xs">
          Auto-complete Enabled
        </Badge>
        <Badge variant="outline" className="text-xs">
          Real-time Indexing
        </Badge>
      </div>
    </div>
  )
}
