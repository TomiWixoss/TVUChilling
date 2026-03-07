import { Button } from '@/components/ui/button'
import { OCRDialog } from '@/components/OCRDialog'
import { useUnityBridge } from '@/hooks/useUnityBridge'
import { useOCRStore } from '@/store/useOCRStore'
import { Toaster } from '@/components/ui/sonner'

export function App() {
  // Initialize Unity bridge
  useUnityBridge()

  const { openDialog } = useOCRStore()

  // Test function - Giả lập Unity gọi
  const handleTestOCR = () => {
    openDialog('NGUYỄN VĂN A')
  }

  return (
    <div className="flex min-h-svh items-center justify-center p-6 bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-neutral-950 dark:to-neutral-900">
      <div className="flex flex-col items-center gap-6 text-center">
        <div>
          <h1 className="text-4xl font-bold mb-2">TVU AR Graduation</h1>
          <p className="text-lg text-muted-foreground">
            React + Vite + TypeScript + Tailwind + ShadcnUI
          </p>
        </div>

        <div className="flex flex-col gap-3">
          <Button
            onClick={handleTestOCR}
            size="lg"
            className="h-14 px-8 text-lg"
          >
            🧪 Test OCR Dialog
          </Button>

          <p className="text-sm text-muted-foreground">
            Click để test popup xác nhận tên OCR
          </p>
        </div>

        <div className="mt-8 p-4 bg-white dark:bg-neutral-900 rounded-lg border">
          <p className="text-xs text-muted-foreground font-mono">
            Unity Bridge: <span className="text-green-600">✓ Ready</span>
          </p>
          <p className="text-xs text-muted-foreground font-mono">
            Call: <code>window.showOCRDialog("TÊN")</code>
          </p>
        </div>
      </div>

      {/* OCR Dialog */}
      <OCRDialog />

      {/* Toast notifications */}
      <Toaster />
    </div>
  )
}

export default App
