import { useOCRStore } from '@/store/useOCRStore'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { toast } from 'sonner'

export function OCRDialog() {
  const { isOpen, studentName, rawOCRText, closeDialog, setStudentName } = useOCRStore()

  const handleCopyRawText = () => {
    navigator.clipboard.writeText(rawOCRText)
    toast.success('Đã copy raw OCR text!')
  }

  const handleConfirm = () => {
    if (window.Unity) {
      window.Unity.call('onNameConfirmed', studentName)
    }
    console.log('✅ Confirmed name:', studentName)
    
    if (window.Unity) {
      window.Unity.call('onDialogClosed', '')
    }
    
    closeDialog()
  }

  const handleCancel = () => {
    console.log('❌ Cancelled')
    
    if (window.Unity) {
      window.Unity.call('onDialogClosed', '')
    }
    
    closeDialog()
  }

  return (
    <Dialog open={isOpen} onOpenChange={closeDialog}>
      <DialogContent className="sm:max-w-[600px] bg-white dark:bg-neutral-950 max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-3xl text-center">
            Hệ thống nhận diện bạn là:
          </DialogTitle>
          <DialogDescription className="text-center text-lg">
            Vui lòng kiểm tra và xác nhận tên của bạn
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="name" className="text-lg">
              Tên của bạn
            </Label>
            <Input
              id="name"
              value={studentName}
              onChange={(e) => setStudentName(e.target.value)}
              className="h-12 text-xl"
              placeholder="Nhập tên của bạn"
            />
          </div>

          {/* Debug: Raw OCR Text */}
          <div className="grid gap-2 border-t pt-4">
            <div className="flex items-center justify-between">
              <Label className="text-sm text-muted-foreground">
                Raw OCR Text (Debug)
              </Label>
              <Button
                variant="ghost"
                size="sm"
                onClick={handleCopyRawText}
                className="h-8"
              >
                Copy
              </Button>
            </div>
            <textarea
              readOnly
              value={rawOCRText}
              className="w-full h-32 p-3 text-sm font-mono bg-neutral-100 dark:bg-neutral-900 border rounded-md resize-none"
              placeholder="Raw OCR text sẽ hiện ở đây..."
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:gap-0">
          <Button
            variant="outline"
            onClick={handleCancel}
            className="h-12 text-lg"
          >
            Hủy
          </Button>
          <Button
            onClick={handleConfirm}
            className="h-12 text-lg bg-green-600 hover:bg-green-700"
          >
            Kích hoạt
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
