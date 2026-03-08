import { motion } from 'framer-motion'
import { 
  Trash2, Copy, Move, X 
} from 'lucide-react'

interface EditPanelProps {
  onDelete: () => void
  onDuplicate: () => void
  onDeselect: () => void
  selectedObjectName?: string
}

export function EditPanel({ 
  onDelete, 
  onDuplicate, 
  onDeselect,
  selectedObjectName 
}: EditPanelProps) {
  return (
    <motion.div
      initial={{ y: '100%' }}
      animate={{ y: 0 }}
      exit={{ y: '100%' }}
      transition={{ type: 'spring', damping: 30, stiffness: 300 }}
      className="fixed bottom-0 left-0 right-0 bg-gradient-to-t from-black via-black/95 to-black/90 backdrop-blur-xl z-50 rounded-t-3xl shadow-2xl"
    >
      {/* Handle Bar */}
      <div className="flex items-center justify-center py-3">
        <div className="w-12 h-1.5 bg-white/30 rounded-full" />
      </div>

      {/* Header */}
      <div className="px-6 pb-4 flex items-center justify-between">
        <div>
          <h3 className="text-white font-semibold text-lg">Chỉnh sửa</h3>
          {selectedObjectName && (
            <p className="text-white/60 text-sm">{selectedObjectName}</p>
          )}
        </div>
        <button
          onClick={onDeselect}
          className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center active:scale-95 transition-transform"
        >
          <X className="w-5 h-5 text-white" />
        </button>
      </div>

      {/* Transform Hints */}
      <div className="px-6 pb-4">
        <div className="bg-blue-500/20 border border-blue-500/30 rounded-xl p-3">
          <div className="flex items-start gap-2">
            <Move className="w-5 h-5 text-blue-400 flex-shrink-0 mt-0.5" />
            <div className="text-sm text-white/80">
              <p className="font-medium text-white mb-1">Cách di chuyển:</p>
              <p>• Kéo trên màn hình để di chuyển</p>
              <p>• Pinch để phóng to/thu nhỏ</p>
              <p>• Xoay 2 ngón để xoay object</p>
            </div>
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="px-6 pb-8">
        <div className="grid grid-cols-2 gap-3">
          {/* Duplicate */}
          <button
            onClick={onDuplicate}
            className="flex items-center justify-center gap-2 bg-blue-500/80 hover:bg-blue-500 active:scale-95 transition-all rounded-xl py-4 shadow-lg"
          >
            <Copy className="w-5 h-5 text-white" />
            <span className="text-white font-medium">Nhân bản</span>
          </button>

          {/* Delete */}
          <button
            onClick={onDelete}
            className="flex items-center justify-center gap-2 bg-red-500/80 hover:bg-red-500 active:scale-95 transition-all rounded-xl py-4 shadow-lg"
          >
            <Trash2 className="w-5 h-5 text-white" />
            <span className="text-white font-medium">Xóa</span>
          </button>
        </div>
      </div>
    </motion.div>
  )
}
