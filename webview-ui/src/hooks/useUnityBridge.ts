import { useEffect } from 'react'
import { useOCRStore } from '@/store/useOCRStore'

// Extend Window interface để TypeScript không báo lỗi
declare global {
  interface Window {
    Unity?: {
      call: (method: string, data: string) => void
    }
    showOCRDialog?: (name: string, rawText?: string) => void
  }
}

export function useUnityBridge() {
  const { openDialog } = useOCRStore()

  useEffect(() => {
    // Expose function cho Unity gọi
    window.showOCRDialog = (name: string, rawText?: string) => {
      console.log('📱 Unity called showOCRDialog:', { name, rawText })
      openDialog(name, rawText)
    }

    console.log('✅ Unity bridge initialized')

    // Cleanup
    return () => {
      delete window.showOCRDialog
    }
  }, [openDialog])
}
