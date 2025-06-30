import os
import shutil
import json
import logging

def load_file_mapping(mapping_file):
    try:
        with open(mapping_file, 'r', encoding='utf-8') as map_file:
            return json.load(map_file)
    except FileNotFoundError:
        logging.error(f"Mapping file not found: {mapping_file}")
        return None
    except json.JSONDecodeError:
        logging.error(f"Error decoding JSON from the mapping file: {mapping_file}")
        return None

def restore_file(review_path, original_path):
    if not os.path.exists(review_path):
        logging.warning(f"File not found in review folder: {review_path}. Skipping restoration.")
        return "Deleted File"

    try:
        os.makedirs(os.path.dirname(original_path), exist_ok=True)
        shutil.move(review_path, original_path)
        logging.info(f"Restored {original_path} from review folder.")
        return "Valid File"
    except Exception as e:
        logging.error(f"Error restoring {original_path} from review folder: {e}")
        return "Restore Error"

def restore_files_from_review(review_folder, mapping_file, status_log_path):
    file_mapping = load_file_mapping(mapping_file)
    if not file_mapping:
        return

    status_log = {"restored": []}

    for review_relative_path, original_path in file_mapping.items():
        review_path = os.path.join(review_folder, review_relative_path)
        status = restore_file(review_path, original_path)

        status_log["restored"].append({
            "file": original_path,
            "status": status
        })

    with open(status_log_path, 'w', encoding='utf-8') as status_file:
        json.dump(status_log, status_file, indent=2)

def main():
    logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

    review_folder = 'D:\\IndTrace\\Dev\\IndTraceV2025\\Src\\Review'
    mapping_file = os.path.join(review_folder, 'file_mapping.json')
    status_log_path = os.path.join(review_folder, 'restoration_status.json')

    restore_files_from_review(review_folder, mapping_file, status_log_path)

if __name__ == "__main__":
    main()