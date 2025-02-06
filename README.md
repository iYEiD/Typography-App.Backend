# Typography
Typography is a web application designed to help users upload and search for images using AI-powered descriptions. It allows users to describe images based on the content (scenes, people, or historical events), and those descriptions are indexed for easy retrieval. The app integrates AI models to analyze images and ElasticSearch to provide fast, full-text search results.

# Features
- Image Upload: Users can upload images they want to store and search.
- AI-Powered Descriptions: AI models analyze images to describe the scene, people, and even identify if the image is related to historical events.
- Search Functionality: Search for images by typing in a description. ElasticSearch is used to quickly retrieve images based on the descriptions stored.
- Secure Storage: Uploaded images are stored in a Minio-based object storage system, ensuring reliable and scalable storage.
- Dockerized: The app is containerized using Docker for easy deployment and scalability.

# Tech Stack
- Frontend: React
- Backend: .NET C# (ASP.NET Core)
- Containerization: Docker
- Object Storage: Minio (for storing images)
- Search Engine: ElasticSearch API (for indexing and searching descriptions)
- AI Models: Used for image recognition and generating descriptive text (OPENAI & GEMINI)

# How It Works
- Upload an Image: Users upload their beloved image via the frontend interface.
- Image Analysis: Once uploaded, AI models are used to analyze the image. The system detects the scene, identifies people, and checks for any historical events.
- Description Storage: If a historical event is identified, the description is stored in ElasticSearch for later search.
- Search for Images: Users can search for images by typing a description of the scene, person, or event, and the app will return relevant results based on the stored descriptions.
