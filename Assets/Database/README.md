Last test: January 7, 2024 21:59

# csharpInfoAPI
Data script uploader, API tutorial, and Windows information gatherer wrote in C#. The gatherer collects the information from the system, and the script (ps1) uploads it to a jsonbin.io API.

......................................

At JSONBIN.IO (Create Bin and X-ACCESS-KEY)

Create an account

Go to BINS

Create a Bin

You can choose (lock flag) if the the Bin is private or public. In this case it will be public.

In Settings (engine flag) we can choose a Name and add the bin to a collection. We will do none of them. Let it without name (Untitled) and without collection.

Bin cannot be blank. So just insert a sample value:
{"sample": "Hello Worlde"}

At copy button (up right) copy the Bin URL. Will be similar to this:

https://api.jsonbin.io/v3/b/64vf2232b89tra2299c3344h

Go to menu API KEYS

Click on Create Access Key

Choose a name for the Access Key

Check the boxes: Read, Update

click on Save Access Key

Copy the API value. Will be something like:

$2b$43$GWzx1ASdvR./J2oplsoPg.aSnJOshsyVzvji/quVISrgJSL/LsF

..................................

EXECUTION

compile the C# file at the project folder. The command:

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:gatherer.exe gatherer.cs

Obs.: check if the .NET version installed at your computer is really like this example, if not, get the right path.

run gatherer.exe. I will generate a file called data.json.

with a text editor or ide, open the script invoker.ps1.

accordingly, replace the values at the variables $apiKey and $url

run the script invoker.ps1. It will collect the data generated and send to your API.

.................................

CHECK IF IT WORKED

There are two ways to confirm if it worked well.

One of them: Go back to your jsonbin.io page. Go to Bins an click at your bin. The data must be seen.

Another way: open your bin URL at your web browser. The data must be seen on the screen as well.
