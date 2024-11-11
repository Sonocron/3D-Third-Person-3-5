=== duckmen ===
{island_accept: -> island_follow_up}
{island.decline: -> island_retry}
-> island

= island
Duckmen: Hey there fellow humanoid! What brings you to these lands?
Robot: Just passing through, looking at the scenery.
Duckmen: Then you might want to check out a pretty cool island over there. It has a lovely small forest on it.
* [Sure! sounds good!] -> island_accept
* (decline) [I rather not.]
    Duckmen: That's a bummer.
    Well if you reconsider, just say the word.
    -> END
    
= island_retry
Robot: About that island...
Duckmen: Did you reconsider? Just say the word, and I'll tell you how to get over there.
* [Yes] -> island_accept
+ [I still don't want to.]
    Duckmen: To each their own...
    -> END

= island_accept
Duckmen: Great. Just head over to the bridge.
There should be a button near it you can press to extend the bridge.
It's a bit stuck, so you really have to jam it in to activate.
Robot: Thanks for the info.
~ Event("unlock_bridge")
-> END

= island_follow_up
{island_apples: -> island_done}
Duckmen: Did you already check out the island?
//{Get_State("item_apple") and not island_apples: -> island_apples}
Just head over to the brige and press the button.
-> END


= island_apples
Robot: Yes, I even found some apples in the forest!
Duckmen: Nice!
Can I have one?
* [Yes]
    Robot: Sure, have some.
//    ~ Add_State("item_apple", -1)
    Duckmen: Thanks!<br><i>munch munch munch</i>
* [No]
    Robot: No, they're mine.
    Duckmen: Oh ok.
- -> END

= island_done
Duckmen: It <b>is</b> a nice island, isn't it?
-> END









