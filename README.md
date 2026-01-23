# AiHelper

## Intention
A tool that shall support almost blind people with tasks like
- recognition of e.g. medicine or documents
- reading and writing mails
- maintaining a shopping list
- read QR codes aloud - purpose in the video below

## Control
It is controlled mainly by voice command using Open AI LLMs.
To "open" up the microphone it is necessary to press the space on the keyboard.

## Available functionality
Currently it can take a picture of what's in front of the camera, read and write emails and maintain a shopping list.
And it can do what Large Language models are good at, e.g. translate to other languages, answer questions etc.

## Videos
I've recorded a video that shows the functionality: https://youtu.be/HTV2MShfgcs

And another video that shows how the intergrated QR code reader can be used to create Braille training material: https://youtu.be/_u7o8dtrkvI

## Open AI
I had initially used a model in Ollama, but that is slow on slower machines.

So I had tried using Open AI online api. That costs fractions of cents per picture that is analyzed, but makes the hardware requirements pretty low.

For this to work an Open AI api key is needed.
The configuration page in the tool has instructions on how to get this. 

## Localization
First user is German, therefore the tool is currenly only in German, but it would be simple to change this e.g. to English.
