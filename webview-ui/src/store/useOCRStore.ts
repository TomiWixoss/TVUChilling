import { create } from 'zustand'

interface OCRStore {
  isOpen: boolean
  studentName: string
  rawOCRText: string
  openDialog: (name: string, rawText?: string) => void
  closeDialog: () => void
  setStudentName: (name: string) => void
}

export const useOCRStore = create<OCRStore>((set) => ({
  isOpen: false,
  studentName: '',
  rawOCRText: '',
  openDialog: (name: string, rawText?: string) => 
    set({ isOpen: true, studentName: name, rawOCRText: rawText || name }),
  closeDialog: () => set({ isOpen: false }),
  setStudentName: (name: string) => set({ studentName: name }),
}))
