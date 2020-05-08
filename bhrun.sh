#!/bin/bash
#Downgrade boost before running the program and upgrade again when done.
version=`pacman -Q --info boost | grep Version | grep -Eo [0-9.-]*`
if [ $version == "1.71.0-4" ]
then :
else echo "Version of boost was $version"
     sudo pacman -U --noconfirm /var/cache/pacman/pkg/boost-1.71.0-4-x86_64.pkg.tar.xz /var/cache/pacman/pkg/boost-libs-1.71.0-4-x86_64.pkg.tar.xz
fi

./"$1"

yay -S boost --noconfirm
