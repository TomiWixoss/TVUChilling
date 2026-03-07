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

export function OCRDialog() {
  const { isOpen, studentName, closeDialog, setStudentName } = useOCRStore()

  const handleConfirm = () => {
    // Gửi tên về Unity
    if (window.unityInstance) {
      window.unityInstance.SendMessage(
        'WebViewManager',
        'OnNameConfirmed',
        studentName
      )
    }
    console.log('✅ Confirmed name:', studentName)
    closeDialog()
  }

  const handleCancel = () => {
    console.log('❌ Cancelled')
    closeDialog()
  }

  return (
    <Dialog open={isOpen} onOpenChange={closeDialog}>
      <DialogContent className="sm:max-w-[600px] bg-white dark:bg-neutral-950">
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
