"use client"

import type React from "react"

import { useState, useCallback } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent } from "@/components/ui/card"
import { Progress } from "@/components/ui/progress"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Upload, FileText, CheckCircle, AlertCircle } from "lucide-react"
import type { SearchEngine } from "@/lib/search-engine"

interface DocumentUploadProps {
  searchEngine: SearchEngine
  onDocumentIndexed: () => void
}

export function DocumentUpload({ searchEngine, onDocumentIndexed }: DocumentUploadProps) {
  const [uploading, setUploading] = useState(false)
  const [progress, setProgress] = useState(0)
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null)

  const handleFileUpload = useCallback(
    async (event: React.ChangeEvent<HTMLInputElement>) => {
      const files = event.target.files
      if (!files || files.length === 0) return

      setUploading(true)
      setProgress(0)
      setMessage(null)

      try {
        for (let i = 0; i < files.length; i++) {
          const file = files[i]
          setProgress((i / files.length) * 100)

          // Simulate processing delay
          await new Promise((resolve) => setTimeout(resolve, 500))

          const content = await readFileContent(file)
          const document = {
            id: `doc_${Date.now()}_${i}`,
            title: file.name,
            content: content,
            type: file.type || "text/plain",
            size: content.length,
            lastModified: new Date(file.lastModified),
          }

          await searchEngine.indexDocument(document)
          onDocumentIndexed()
        }

        setProgress(100)
        setMessage({
          type: "success",
          text: `Successfully indexed ${files.length} document(s)`,
        })
      } catch (error) {
        setMessage({
          type: "error",
          text: `Error indexing documents: ${error instanceof Error ? error.message : "Unknown error"}`,
        })
      } finally {
        setUploading(false)
        // Reset file input
        event.target.value = ""
      }
    },
    [searchEngine, onDocumentIndexed],
  )

  const readFileContent = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.onload = (e) => {
        const content = e.target?.result as string
        resolve(content)
      }
      reader.onerror = () => reject(new Error("Failed to read file"))
      reader.readAsText(file)
    })
  }

  return (
    <div className="space-y-4">
      <Card className="border-dashed border-2 border-gray-300 hover:border-gray-400 transition-colors">
        <CardContent className="p-6">
          <div className="text-center">
            <Upload className="mx-auto h-12 w-12 text-gray-400 mb-4" />
            <Label htmlFor="file-upload" className="cursor-pointer">
              <div className="text-lg font-medium text-gray-900 mb-2">Upload Documents</div>
              <div className="text-sm text-gray-500 mb-4">Supports TXT, HTML, XML, and other text-based formats</div>
              <Button variant="outline" disabled={uploading}>
                <FileText className="mr-2 h-4 w-4" />
                Choose Files
              </Button>
            </Label>
            <Input
              id="file-upload"
              type="file"
              multiple
              accept=".txt,.html,.xml,.json,.md"
              onChange={handleFileUpload}
              className="hidden"
            />
          </div>
        </CardContent>
      </Card>

      {uploading && (
        <Card>
          <CardContent className="p-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span>Indexing documents...</span>
                <span>{Math.round(progress)}%</span>
              </div>
              <Progress value={progress} className="w-full" />
            </div>
          </CardContent>
        </Card>
      )}

      {message && (
        <Alert variant={message.type === "error" ? "destructive" : "default"}>
          {message.type === "success" ? <CheckCircle className="h-4 w-4" /> : <AlertCircle className="h-4 w-4" />}
          <AlertDescription>{message.text}</AlertDescription>
        </Alert>
      )}
    </div>
  )
}
