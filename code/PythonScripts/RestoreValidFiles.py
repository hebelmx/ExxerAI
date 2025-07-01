import os
import shutil
import json
import logging

def load_file_mapping(mapping_file):
    """Loads the file mapping from a JSON file."""
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
    """Restores a single file to its original location if it exists."""
    if not os.path.exists(review_path):
        logging.warning(f"File not found in review folder: {review_path}. Skipping restoration.")
        return

    try:
        os.makedirs(os.path.dirname(original_path), exist_ok=True)
        shutil.move(review_path, original_path)
        logging.info(f"Restored {original_path} from review folder.")
    except Exception as e:
        logging.error(f"Error restoring {original_path} from review folder: {e}")

def restore_files_from_review(review_folder, mapping_file):
    """Restores files from the review folder to their original locations using the mapping file."""
    file_mapping = load_file_mapping(mapping_file)
    if not file_mapping:
        return

    for review_relative_path, original_path in file_mapping.items():
        review_path = os.path.join(review_folder, review_relative_path)
        restore_file(review_path, original_path)

def main():
    # Configure logging
    logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

    # Replace these paths with your actual paths
    review_folder = 'D:\\IndTrace\\Dev\\IndTraceV2025\\Src'
    review_folder = 'D:\\IndTrace\\Dev\\IndTraceV2025\\Src\\Review'
    mapping_file = os.path.join(review_folder, 'file_mapping.json')
    
    restore_files_from_review(review_folder, mapping_file)

if __name__ == "__main__":
    main()
