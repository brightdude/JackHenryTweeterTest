# JackHenryTweeterTest
**JackHenryTweeterTest**

This assignment presented quite a few challenges and it was a lot of fun to code.
I chose to use Azure durable functions to host the solution as it provides a multitude of features that lend itself to the challenges of this assignment, 
**namely:**
1.	Ability to run without timeout limitations of regular azure functions. 
2.	No need to run as uncool console app,  as windows service would require installation that most of us forgot how to do. (it's installutil <yourexecutable>.exe )
3.	To be relatively thread safe and memory resident for the real-time changing repository of tweets.

Thus, I’ve chosen Azure Durable Serverless Functions provide elegant set of features without much complexity.  There is no hosting model choice needs to be specified, as Durable functions have stable state aware run time and VM destruction safe behind the scenes by Azure 

  I chose not to add "Guard.Against" libraries for null checking as its adds confusion, although they are standard in my coding practices. 

  I ran out of personal time and did not add any unit tests for the Services, **deduct points for that if you must**.  
But I did reviewed most places where error checking is needed in more then happy-path approach.

As well I addressed many points of the assignment in the comments in the code, as I belive thats the best place for them to be.
