# TalkGPT 

![Dialog with Nicole](./dialog_with_nicole.png)

## Description

Viewers can communicate directly with OpenAI, with a set number of dialogs stored in a history. I have extended the original implementation with a history per recognized user, so that the context can be improved in dialogue with the AI.

 ## Contributors

The original ChatGPT-action was created by Fluffless (flufflessmc @Discord)

## Import-File

talkGPT.sb

## Installation

1. Download the file above and drag&drop it into the "Import String" in StreamerBot.

2. Go into the imported action, edit the "Set Argument" that has "OPEN_API_KEY" in it. You can get your API-key at https://platform.openai.com/api-keys.

3. Change the behavior of your AI by adjusting the corresponding argument.

4. Determine the number of remembered dialogs by setting the argument "maxHistory"

5. If speaker.bot is not used, deactivate the corresponding line. Otherwise, store the voice you have set up here.

4. **Done!** :partying_face:

[wrap="info"]
Note: In order for the history to work correctly, I had to access the gpt-4 model. Older models such as gpt-3.5-turbo did not work. You could also change the number of tokens in the code itself. 
[/wrap]

<div data-theme-toc="true"> </div>