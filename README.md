This project spawned from the idea that I wanted a better way to handle tickets to some theatre shows I go to.
They came in the form of a PDF file.  They had always resides on my phone in the downloads folder and I wanted a 
better way to track them.

The final approach was using a solution from Telerik Hybrid Blazor App. This brought in a Telerick PDF Viewer.
Telerick does a PDF viewer specifically built for .NET MAUI without the need for Blazor but it doesn't display the PDF correctly.

I also give a shoot out to iText as I'm using their library to parse out the PDF file and see if I can intelligently
infer from the list of tags that gets exported as to what type of document this is.



