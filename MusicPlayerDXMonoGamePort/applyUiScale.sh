﻿#!/usr/bin/env sh

UISCALE=2.5

# C# Code
sed -i "11s/.*/        public const float scaleMult = ${UISCALE}f;/" ./HelperClasses/UiScaling.cs

# Font Sizes
SIZE="$(awk "BEGIN {printf \"%.0f\", 20*$UISCALE}")"
sed -i "20s/.*/    <Size>$SIZE<\/Size>/" ./Content/Title.spritefont
SIZE="$(awk "BEGIN {printf \"%.0f\", 14*$UISCALE}")"
sed -i "20s/.*/    <Size>$SIZE<\/Size>/" ./Content/Font.spritefont