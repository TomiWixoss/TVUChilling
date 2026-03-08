import { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Box, Circle, Cylinder, Cone, 
  ChevronDown, ChevronUp,
  X
} from 'lucide-react'

interface Model {
  id: string
  name: string
  icon: React.ComponentType<{ className?: string }>
  category: string
}

interface ModelSelectorProps {
  isExpanded: boolean
  onToggle: () => void
  onModelSelect: (modelId: string) => void
  selectedModel: string | null
  trackingEnabled: boolean
}

const MODELS: Model[] = [
  // Basic Shapes
  { id: 'none', name: 'None', icon: X, category: 'basic' },
  { id: 'cube', name: 'Cube', icon: Box, category: 'basic' },
  { id: 'sphere', name: 'Sphere', icon: Circle, category: 'basic' },
  { id: 'cylinder', name: 'Cylinder', icon: Cylinder, category: 'basic' },
  { id: 'cone', name: 'Cone', icon: Cone, category: 'basic' },
]

const CATEGORIES = [
  { id: 'basic', name: 'Basic', icon: Box },
  // Add more categories as needed
]

export function ModelSelector({ 
  isExpanded, 
  onToggle, 
  onModelSelect, 
  selectedModel,
  trackingEnabled 
}: ModelSelectorProps) {
  const [activeCategory, setActiveCategory] = useState('basic')

  const filteredModels = MODELS.filter(m => {
    if (m.category === 'tracked') {
      return trackingEnabled
    }
    return m.category === activeCategory
  })

  const handleModelClick = (modelId: string) => {
    onModelSelect(modelId)
    // Auto collapse after selection (except None)
    if (modelId !== 'none') {
      setTimeout(() => onToggle(), 300)
    }
  }

  return (
    <>
      {/* Backdrop */}
      <AnimatePresence>
        {isExpanded && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-40"
            onClick={onToggle}
          />
        )}
      </AnimatePresence>

      {/* Bottom Sheet */}
      <motion.div
        className="fixed bottom-0 left-0 right-0 bg-gradient-to-t from-black via-black/95 to-black/90 backdrop-blur-xl z-50 rounded-t-3xl shadow-2xl"
        initial={false}
        animate={{
          y: isExpanded ? 0 : 'calc(100% - 80px)',
        }}
        transition={{
          type: 'spring',
          damping: 30,
          stiffness: 300,
        }}
      >
        {/* Handle Bar */}
        <div 
          className="flex items-center justify-center py-3 cursor-pointer"
          onClick={onToggle}
        >
          <div className="w-12 h-1.5 bg-white/30 rounded-full" />
        </div>

        {/* Category Tabs - Always Visible */}
        <div className="px-6 pb-3">
          <div className="flex items-center gap-3 overflow-x-auto scrollbar-hide">
            {CATEGORIES.map((cat) => {
              const Icon = cat.icon
              return (
                <button
                  key={cat.id}
                  onClick={() => {
                    setActiveCategory(cat.id)
                    if (!isExpanded) onToggle()
                  }}
                  className={`flex items-center gap-2 px-4 py-2 rounded-full whitespace-nowrap transition-all ${
                    activeCategory === cat.id
                      ? 'bg-blue-500 text-white'
                      : 'bg-white/10 text-white/70'
                  }`}
                >
                  <Icon className="w-4 h-4" />
                  <span className="text-sm font-medium">{cat.name}</span>
                </button>
              )
            })}
            
            {/* Expand/Collapse Button */}
            <button
              onClick={onToggle}
              className="ml-auto flex items-center gap-2 px-4 py-2 rounded-full bg-white/10 text-white/70"
            >
              {isExpanded ? (
                <>
                  <ChevronDown className="w-4 h-4" />
                  <span className="text-sm">Thu gọn</span>
                </>
              ) : (
                <>
                  <ChevronUp className="w-4 h-4" />
                  <span className="text-sm">Mở rộng</span>
                </>
              )}
            </button>
          </div>
        </div>

        {/* Model Grid - Only when expanded */}
        <AnimatePresence>
          {isExpanded && (
            <motion.div
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
              className="px-6 pb-8 overflow-hidden"
            >
              <div className="grid grid-cols-4 gap-4 max-h-[50vh] overflow-y-auto">
                {filteredModels.map((model) => {
                  const Icon = model.icon
                  const isSelected = selectedModel === model.id
                  
                  return (
                    <button
                      key={model.id}
                      onClick={() => handleModelClick(model.id)}
                      className={`aspect-square rounded-2xl flex flex-col items-center justify-center gap-2 transition-all ${
                        isSelected
                          ? 'bg-blue-500 border-2 border-white shadow-lg scale-105'
                          : 'bg-white/10 border-2 border-white/20 hover:bg-white/20'
                      }`}
                    >
                      <Icon className={`w-8 h-8 ${isSelected ? 'text-white' : 'text-white/80'}`} />
                      <span className={`text-xs ${isSelected ? 'text-white font-medium' : 'text-white/70'}`}>
                        {model.name}
                      </span>
                    </button>
                  )
                })}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </motion.div>
    </>
  )
}
