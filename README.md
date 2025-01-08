# Alcedo

## Overview

Alcedo is a .NET MAUI application designed to generate descriptive tags for images. The application allows users to load images, either from their device or by taking a photo, and then uses OpenAI's GPT model to generate relevant tags for the image. These tags can be copied and used for social media or other purposes.

The main features of Alcedo include:
- Loading images from the device or capturing photos using the camera.
- Compressing images to ensure they are within size limits.
- Generating descriptive tags for images using OpenAI's GPT model.
- Copying generated tags to the clipboard for easy use.

The project is structured into several key components:
- **MainPage.xaml**: Defines the user interface for the main page of the application.
- **MainPage.xaml.cs**: Contains the code-behind for the main page, handling user interactions and image processing.
- **ComputerVisionService.cs**: Interacts with the OpenAI API to generate tags for images.

Alcedo aims to simplify the process of generating and managing tags for images, making it easier for users to enhance their social media posts or organize their image collections.
