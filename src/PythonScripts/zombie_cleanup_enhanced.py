import os
import shutil
import json
import logging

def setup_logging(log_file="zombie_cleanup.log"):
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s [%(levelname)s] %(message)s',
        handlers=[
            logging.FileHandler(log_file, encoding='utf-8'),
            logging.StreamHandler()
        ]
    )

def load_valid_file_flags(status_log_path):
    valid_files = set()
    if os.path.exists(status_log_path):
        try:
            with open(status_log_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                for entry in data.get("restored", []):
                    if entry.get("status") == "Valid File":
                        valid_files.add(os.path.abspath(entry["file"]))
        except Exception as e:
            logging.warning(f"Could not load restoration status: {e}")
    return valid_files

def find_cs_files(directory):
    cs_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith('.cs'):
                cs_files.append(os.path.join(root, file))
    return cs_files

def is_comment_only_file(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as file:
            lines = [line.strip() for line in file if line.strip()]
            if not lines:
                return False
            if all(line.startswith("//") for line in lines):
                return True
            if lines[0].startswith("/*") and lines[-1].endswith("*/"):
                return True
    except Exception as e:
        logging.error(f"Error analyzing comments in {file_path}: {e}")
    return False

def is_file_less_than_n_lines(file_path, max_lines=4):
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as file:
            lines = [line.strip() for line in file if line.strip()]
            if len(lines) >= max_lines:
                return False

            joined = " ".join(lines).lower()
            keywords = ["interface ", "delegate ", "record ", "exception "]
            return not any(k in joined for k in keywords)
    except Exception as e:
        logging.error(f"Error processing {file_path}: {e}")
        return False

def move_files_to_review(file_paths, review_folder, mapping_file):
    if not os.path.exists(review_folder):
        os.makedirs(review_folder)

    file_mapping = {}
    moved_count = 0
    for file_path in file_paths:
        try:
            destination_path = os.path.join(review_folder, os.path.relpath(file_path, os.path.commonpath(file_paths)))
            file_mapping[destination_path] = file_path
            os.makedirs(os.path.dirname(destination_path), exist_ok=True)
            shutil.move(file_path, destination_path)
            logging.info(f"Moved {file_path} to {destination_path}")
            moved_count += 1
        except Exception as e:
            logging.error(f"Error moving {file_path} to {review_folder}: {e}")

    with open(mapping_file, 'w', encoding='utf-8') as map_file:
        json.dump(file_mapping, map_file, indent=4)

    return moved_count

def main():
    setup_logging()

    directory_to_search = 'D:\\IndTrace\\Dev\\IndTraceV2025\\Src\\'
    review_folder_name = 'review'
    mapping_file_name = 'file_mapping.json'
    status_log_path = os.path.join(directory_to_search, review_folder_name, 'restoration_status.json')

    review_folder_path = os.path.join(directory_to_search, review_folder_name)
    mapping_file_path = os.path.join(review_folder_path, mapping_file_name)

    valid_files = load_valid_file_flags(status_log_path)

    logging.info("Starting zombie file cleanup scan...")
    cs_files = find_cs_files(directory_to_search)
    logging.info(f"Found {len(cs_files)} C# files.")

    files_to_review = []
    for cs in cs_files:
        abs_path = os.path.abspath(cs)
        if abs_path in valid_files:
            continue
        if is_file_less_than_n_lines(cs, max_lines=4) or is_comment_only_file(cs):
            files_to_review.append(cs)

    if files_to_review:
        moved_count = move_files_to_review(files_to_review, review_folder_path, mapping_file_path)
        logging.info(f"Moved {moved_count} files to the review folder.")
    else:
        logging.info("No files to review found.")

    logging.info("Zombie file cleanup scan complete.")

if __name__ == "__main__":
    main()