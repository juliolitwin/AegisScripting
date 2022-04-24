# Aegis Scripting

## About

Scripting lib converted from C++ to C#.

## Why

It is functional and tested, but not ideal. I'm publishing because I ended up rewriting a new library for my script, using ANTLR. So in case you fall here with a parachute, here's the tip.

It's just a scripting library, so all the backend implementation you need to implement yourself.

## Script example

```
npc "some_map" "NPC_NAME" SPRITE_TYPE _POSITION_X_ _POSITION_X_ _DIRECTION_ _WIDTH_ _HEIGHT_
OnClick:
	dialog "[NPC_NAME]"
	dialog "Welcome to"
	dialog "How may I help you?"
	wait
	choose menu "Option 1" "Option 2" "Cancel"
	case 1
		dialog "[NPC_NAME]"
		dialog "You choice option 1"
		close
	break
	case 2
		dialog "[NPC_NAME]"
		dialog "You choice option 2"
	break
	case 3
	break
	endchoose
return
```
