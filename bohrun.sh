#!/bin/bash
version=`pacman -Q --info boost | grep Version | grep -Eo [0-9.-]*`
if [ $version == "1.71.0-4" ]
then :
else echo "Version of boost was $version"
     sudo pacman -U --noconfirm /var/cache/pacman/pkg/boost-1.71.0-4-x86_64.pkg.tar.xz /var/cache/pacman/pkg/boost-libs-1.71.0-4-x86_64.pkg.tar.xz
fi
version=`acman -Q --info libffi | grep Version| grep -Eo [0-9.-]*`
if [ $version == "3.2.1-4" ]
then :
else echo "Version of libffi was $version"
     sudo pacman -U --noconfirm /var/cache/pacman/pkg/libffi-3.2.1-4-x86_64.pkg.tar.xz
fi

./"$1"

yay -S boost libffi --noconfirm
