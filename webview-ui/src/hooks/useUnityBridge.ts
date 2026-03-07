import { useEffect } from 'react'
import { useOCRStore } from '@/store/useOCRStore'

// Extend Window interface để TypeScript không báo lỗi
declare global {
  interface Window {
    unityInstance?: {
      SendMessage: (objectName: string, methodName: string, value: string) => void
    }
    showOCRDialog?: (name: string) => void
  }
}

export function useUnityBridge() {
  const { openDialog } = useOCRStore()

  useEffect(() => {
    // Expose function cho Unity gọi
    window.showOCRDialog = (name: string) => {
      console.log('📱 Unity called showOCRDialog:', name)
      openDialog(name)
    }

    console.log('✅ Unity bridge initialized')

    // Cleanup
    return () => {
      delete window.showOCRDialog
    }
  }, [openDialog])
}
