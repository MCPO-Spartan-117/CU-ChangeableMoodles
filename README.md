# ChangeableMoodles

A small BepInEx plugin for Casualties Unknown to modify `MoodleManager::AddMoodle` to inject user moodles via a transpiler and case insensitive dictionary.

## Overview 
Moodles are scanned recursively in `$(AssemblyPath)/Moodles`,\
Changes moodle sprite based on intensity and supports CUCoreLib added moodles with a `cucorelib.` file name prefix,\
Supports only PNGs, image is resized based on width and likely won't look right if it isn't uniform with height,\
File name format is `[cucorelib.]$(internal_moodle_name)[$(intensity)-$(intensity_range)].png`,\
Names will be appended with `0` if they lack a number,\
Config option in `Video` to hotload moodles.
