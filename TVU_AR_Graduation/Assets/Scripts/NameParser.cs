using System.Linq;

public static class NameParser
{
    public static string ExtractStudentName(string ocrText)
    {
        if (string.IsNullOrEmpty(ocrText))
        {
            return "SINH VIÊN";
        }
        
        // Tách thành các dòng
        string[] lines = ocrText.Split('\n');
        
        // Tìm dòng chứa "Cử nhân" hoặc "CỬ NHÂN"
        for (int i = 0; i < lines.Length - 1; i++)
        {
            string line = lines[i].ToUpper().Trim();
            if (line.Contains("CỬ NHÂN") || line.Contains("CU NHAN"))
            {
                // Lấy dòng tiếp theo (thường là tên)
                string nameCandidate = lines[i + 1].Trim();
                
                // Validate: Tên phải có ít nhất 2 từ
                if (nameCandidate.Split(' ').Length >= 2)
                {
                    return nameCandidate;
                }
            }
        }
        
        // Fallback: Tìm dòng có chữ in hoa và dài nhất (thường là tên)
        string longestUpperLine = "";
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            // Check if mostly uppercase
            if (trimmed.Length > 0 && trimmed.Count(char.IsUpper) > trimmed.Length / 2)
            {
                if (trimmed.Length > longestUpperLine.Length)
                {
                    longestUpperLine = trimmed;
                }
            }
        }
        
        if (!string.IsNullOrEmpty(longestUpperLine))
        {
            return longestUpperLine;
        }
        
        // Last fallback
        return "SINH VIÊN";
    }
}
