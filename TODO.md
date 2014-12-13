auth creates and updates the sessionid/endpoint in the db
other services pull account and use .Equals() to verify it's all legit
    account name
    session id
    remote endpoint

**** BAD COUPLING RIGHT NOW ****

AuthorizedMessage/AuthedMessage or whatever
    just includes the sessionid/account name or something?
    maybe handles the auth part?

No more login messages, just have the client start doing stuff
    and also don't use an Online/Offline set of messages, just set the status (wrt the chat server)

LOOK UP WHAT EXACTLY COUPLING MEANS AND HOW TO BE GOOD ABOUT IT







Move message handler into message processor
    Decouple Session (mostly) from messages
Need a MessagePacketFactory class