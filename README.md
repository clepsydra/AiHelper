# AiHelper

## Intention
A tool that shall support almost blind people recognizing e.g. written texts, medicine packages etc.

## Control
It is controlled using the keyboard and gives audio feedback.

## Available functionality
Currently it can take a picture of something in front of the webcam, send it to Open AI for analysis and tell the user what it is.

## Open AI
I had initially used a model in Ollama, but that is slow on slower machines.

So I had tried using Open AI online api. That costs fractions of cents per picture that is analyzed, but makes the hardware requirements pretty low.

For this to work an Open AI api key is needed.
The configuration page in the tool has instructions on how to get this. 

## Localization
First user is German, therefore the tool is currenly only in German, but it would be very simple to change this.
