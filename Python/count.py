import os

def count_lines(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        return sum(1 for line in file)

def count_lines_in_directory(directory):
    total_lines = 0
    for root, dirs, files in os.walk(directory):
        for file_name in files:
            if file_name.endswith('.cs'):
                file_path = os.path.join(root, file_name)
                total_lines += count_lines(file_path)
    return total_lines

if __name__ == "__main__":
    directory = "../Assets/Scripts/"
    lines_count = count_lines_in_directory(directory)
    print(f"資料夾 {directory} 中的所有 C# 檔案共有 {lines_count} 行")
