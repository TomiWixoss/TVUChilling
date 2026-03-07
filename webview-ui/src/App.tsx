import { OCRDialog } from '@/components/OCRDialog'
import { useUnityBridge } from '@/hooks/useUnityBridge'
import { Toaster } from '@/components/ui/sonner'

export function App() {
  // Initialize Unity bridge
  useUnityBridge()

  return (
    <>
      {/* OCR Dialog - Sẽ hiện khi Unity gọi window.showOCRDialog() */}
      <OCRDialog />

      {/* Toast notifications */}
      <Toaster />
    </>
  )
}

export default App
