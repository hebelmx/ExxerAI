import unittest
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
import datetime

from src.models import Base, Document
from src.database import add_document, update_document_status, mark_vectorized, mark_fine_tuned, document_exists, delete_document

class TestDatabaseFunctions(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        # Use an in-memory SQLite database for testing
        cls.engine = create_engine('sqlite:///:memory:')
        Base.metadata.create_all(cls.engine)
        cls.Session = sessionmaker(bind=cls.engine)
        cls.session = cls.Session()

    def setUp(self):
        # This method will run before each test
        self.session.query(Document).delete()
        self.session.commit()

    def test_add_document(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        doc = self.session.query(Document).filter_by(file_name="test.pdf").first()
        self.assertIsNotNone(doc)
        self.assertEqual(doc.file_name, "test.pdf")
        self.assertEqual(doc.status, "new")

    def test_update_document_status(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        update_document_status("test.pdf", "updated", self.session)
        doc = self.session.query(Document).filter_by(file_name="test.pdf").first()
        self.assertEqual(doc.status, "updated")

    def test_mark_vectorized(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        mark_vectorized("test.pdf", self.session)
        doc = self.session.query(Document).filter_by(file_name="test.pdf").first()
        self.assertTrue(doc.vectorized)
        self.assertEqual(doc.status, "tokenized")

    def test_mark_fine_tuned(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        mark_fine_tuned("test.pdf", self.session)
        doc = self.session.query(Document).filter_by(file_name="test.pdf").first()
        self.assertTrue(doc.fine_tuned)
        self.assertEqual(doc.status, "fine_tuned")

    def test_document_exists(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        self.assertTrue(document_exists("test.pdf", self.session))
        self.assertFalse(document_exists("nonexistent.pdf", self.session))

    def test_delete_document(self):
        add_document("test.pdf", "/path/to/test.pdf", self.session)
        delete_document("test.pdf", self.session)
        doc = self.session.query(Document).filter_by(file_name="test.pdf").first()
        self.assertIsNone(doc)

if __name__ == '__main__':
    unittest.main()
