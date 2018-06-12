#!/usr/bin/python3
# -*- coding: UTF-8 -*-
import argparse
import json
from typing import Dict


KNOWN_PACKAGES_NAMES = [
    "RemoteTech-Antennas",
    "RemoteTech-Common",
    "RemoteTech-Delay",
    "RemoteTech-Transmitter"]

DEFAULT_CONFIG_FILE = "RemoteTech-Complete.json"


class DefaultEncoder(json.JSONEncoder):
    def default(self, o):
        return o.__dict__


class Module(object):
    def __init__(self, name):
        self.name = name
        self.dst_dir = "Plugins"

    @staticmethod
    def from_json(module_dict):
        module = Module(module_dict["name"])
        module.dst_dir = module_dict["dst_dir"]
        return module


class Directory(object):
    def __init__(self, name):
        self.src_dir = name
        self.dst_dir = None
        self.copy_list = list()
        self.exception_list = list()

    @staticmethod
    def from_json(directory_dict):
        directory = Directory(directory_dict["src_dir"])
        directory.dst_dir = directory_dict["dst_dir"]

        copy_list = directory_dict["copy_list"]
        for copy_entry in copy_list:
            directory.copy_list.append(copy_entry)

        exception_list = directory_dict["exception_list"]
        for exception in exception_list:
            directory.exception_list.append(exception)

        return directory


class PackageEntry(object):
    def __init__(self, package_name, init_default_module=True):
        self.package_name = package_name
        self.modules = list()
        self.copyable_directories = list()

        if init_default_module:
            module = Module(package_name + ".dll")
            self.add_module(module)

    def add_copyable_directory(self, directory):
        self.copyable_directories.append(directory)

    def add_module(self, module):
        self.modules.append(module)

    @staticmethod
    def from_json(parsed_dict):
        package_name = parsed_dict["package_name"]
        package = PackageEntry(package_name, False)

        # modules
        modules = parsed_dict["modules"]
        for module_dict in modules:
            module_obj = Module.from_json(module_dict)
            package.add_module(module_obj)

        # directories
        directories = parsed_dict["copyable_directories"]
        for dir_dict in directories:
            directory = Directory.from_json(dir_dict)
            package.add_copyable_directory(directory)

        return package


class PackageEntryJSONDecoder(json.JSONDecoder):
    def decode(self, json_string: str, **kwargs) -> Dict[str, PackageEntry]:
        """
        """
        all_packages = dict()
        all_packages_dict = super().decode(json_string)
        for package_name in all_packages_dict:
            package_dict = all_packages_dict[package_name]
            package = PackageEntry.from_json(package_dict)
            all_packages[package.package_name] = package

        return all_packages


def decode(file_path: str) -> Dict[str, PackageEntry]:
    with open(file_path) as f:
        content = f.read()
        packages = PackageEntryJSONDecoder().decode(content)
        return packages


def encode(file_path: str):
    package_dict = dict()

    #
    # RemoteTech-Antennas
    #

    rt_antennas = PackageEntry("RemoteTech-Antennas", False)

    # copy files in root dir
    rt_antennas_dir = Directory(".")
    patterns = ["*.md", "*.txt", "*.cfg"]
    rt_antennas_dir.copy_list.extend(patterns)
    rt_antennas.add_copyable_directory(rt_antennas_dir)

    # copy parts directory
    rt_antennas_parts = Directory("Parts")
    rt_antennas.add_copyable_directory(rt_antennas_parts)

    package_dict[rt_antennas.package_name] = rt_antennas

    #
    # RemoteTech-Common
    #

    rt_common = PackageEntry("RemoteTech-Common")

    # copy files in root dir
    rt_common_dir = Directory(".")
    patterns = ["*.md"]
    rt_common_dir.copy_list.extend(patterns)
    rt_common.add_copyable_directory(rt_common_dir)
    
    # copy texture directory
    common_textures = Directory("Textures")
    rt_common.add_copyable_directory(common_textures)

    package_dict[rt_common.package_name] = rt_common

    #
    # RemoteTech-Delay
    #

    rt_delay = PackageEntry("RemoteTech-Delay")

    # copy files in root dir
    rt_delay_dir = Directory(".")
    patterns = ["*.md"]
    rt_delay_dir.copy_list.extend(patterns)
    rt_delay.add_copyable_directory(rt_delay_dir)

    # copy texture directory
    delay_textures = Directory("Textures")
    rt_delay.add_copyable_directory(delay_textures)

    package_dict[rt_delay.package_name] = rt_delay

    #
    # RemoteTech-Transmitter
    #

    rt_transmitter = PackageEntry("RemoteTech-Transmitter")

    # copy files in root dir
    rt_transmitter_dir = Directory(".")
    patterns = ["*.md"]
    rt_transmitter_dir.copy_list.extend(patterns)
    rt_transmitter.add_copyable_directory(rt_transmitter_dir)

    package_dict[rt_transmitter.package_name] = rt_transmitter

    with open(file_path, "w") as f:
        json.dump(package_dict, f, cls=DefaultEncoder, sort_keys=True,
                  indent=4, separators=(',', ': '))


def main(args):
    config_type = args.config_type
    if config_type == "encode":
        encode(DEFAULT_CONFIG_FILE)
    elif config_type == "decode":
        package_dict = decode(DEFAULT_CONFIG_FILE)
        # no uses of decoded JSON content are identified yet
    else:
        print("[-] Error: unknown config type: {}".format(config_type))

    print("[*] Job done!")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description='RemoteTech-Complete package configuration builder')

    parser.add_argument(
        "-c", "--config-type", action="store", dest="config_type",
        choices={"encode", "decode"}, default="encode",
        help="Config type [default: encode]:\n\tencode (build config file)"
             "\n\tdecode (read config file).")

    parsed_args = parser.parse_args()

    main(parsed_args)
