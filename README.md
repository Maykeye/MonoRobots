# MonoRobots

BSD Robots inspired game made in MonoGame to get surface knowledge of it.
Built more on xUnit and then just rendered by MonoGame.
Controls, mainly by numpad:

* `NP1`-`NP9`: move around (`NP5` - do nothing)
* `Space` - teleport to random non-occupied cell (non-safe: can teleport next to robot afetr which chop occurs)
* `F12` - restart

Not implemented:
* Game over screen and/or restarting level with more robots
* Calculate and display score

Helper scripts:
* `build.sh` -- restore/rebuild
* `run.sh` -- run the game (I don't bother with xnb, etc; load where `Assets/` are avaialable)
* `test.sh` -- run the tests (if `argv[1]` is specified -- filter tests by the name)
