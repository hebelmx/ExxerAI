from sqlalchemy import create_engine, Column, Integer, String, Boolean, DateTime
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
import datetime

Base = declarative_base()

class Document(Base):
    __tablename__ = 'documents'
    
    id = Column(Integer, primary_key=True)
    file_name = Column(String, nullable=False)
    file_path = Column(String, nullable=False)
    status = Column(String, nullable=False)
    last_modified = Column(DateTime, default=datetime.datetime.utcnow)
    vectorized = Column(Boolean, default=False)
    fine_tuned = Column(Boolean, default=False)

# Create an SQLite database
engine = create_engine('sqlite:///document_status.db')
Base.metadata.create_all(engine)

# Create a session
Session = sessionmaker(bind=engine)
session = Session()
