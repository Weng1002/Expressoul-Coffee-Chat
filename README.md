# Expressoul-Coffee-Chat
About OpenHCI 2025 – Team 3 XRAI: 利用現有AI技術為工具，透過XR互動方式作為媒介，探索未來互動方式

我們這組利用 Meta Quest Pro 配合 Unity 所搭建的 UI/UX ，來時現在現實世界中呈現虛擬的介面來做互動。

## Theme Discussion

![Problem](image/Problem.png)

我們的專案想要是以一個剛步入社會找工作的新鮮人出發，這個人可能是透過某個人某個事情（keyrelation）來跟前輩或是主管創造一小部分的overlapping 的部分，而我們不想要是以產品角度出發，是以改善整個對談流程的系統中扮演agent代理人輔助，不過度干涉使用者。

因為我們透過先前的訪問知道大家在這個coffee chat中的最大問題是，焦慮常常就會沒話題或是讓整體對談變成目標導向，但我們應該反過來思考，也要思考對方的角度，創造我們雙方的印象impression，然後再對談的一開始就是要以提升或昇華雙方的overlapping 未知處去創造未來繼續交流的可能性！

![Review](image/Review.png)

## Related_work

![Related_work](image/Related_work.png)


## User Persona

![Persona](image/Persona.png)

## Engineering 

![enginee_flow](image/enginee_flow.png)

Input 有雙方的背景資料與談話目標(text)、對話音檔(audio)、以及使用者的選擇回饋(interactions)。

這三筆 inputs 會經由 Unity 中的 C# client端，發送 HTTPS Request 這兩筆資料到外部 Python Flask Server。

Flask Server 會去接收請求，然後我們串接 Gemini API，根據所有 input information 包成 Prompt，利用 LLM 判斷使用者可能較有興趣的話題與提示

其中 Gemini 使用到兩個版本模型，2.5 Pro 應用在找共同點，而對話語音或元件更新，則選用快速的2.5 Flash。

由 Gemini 輸出語句提示整理後，打包成一包封包，交由 Server 發回 HTTPS Response，Client 端再去 extracting 關鍵詞句顯示在 Unity UI 上，完成前後端串接。

## Demo

![Demo](image/Demo.png)
