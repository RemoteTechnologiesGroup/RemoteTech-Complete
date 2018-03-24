#!/usr/bin/python3
# -*- coding: UTF-8 -*-
"""
Packager script for RemoteTech-Complete.

Prerequisites:
    - Expect built assemblies (*.dll) and assoicated files in 
        RemoteTech-Complete\src\[sub repositories] directories.

    - Expect RemoteTech-Complete.json of specific files and directories
        of the sub repositories to include into the final output
        directory.

    - Expect GameData folder as final output directory.

    - Working directory is the root directory of the repository
        (RemoteTech-Complete/) unless changed with the --cwd command line
        option.
"""
import logging
import shutil
from typing import Optional, Iterable
import argparse
import os
import glob

import packager_config

PACKAGER_VERSION = 0.3
"""str: program versioning"""

LOGGING_LEVELS = {
    'debug': logging.DEBUG,
    'info': logging.INFO,
    'warning': logging.WARNING,
    'error': logging.ERROR,
    'critical': logging.CRITICAL,
}
"""dict: logging level names to be used with --logging-level command line
option."""

SOURCE_DIR = "src"
"""str: name of the source directory (relative to the root folder)."""

DEFAULT_OUTPUT_DIR = "GameData{0}RemoteTech".format(os.sep)
"""str: default output directory for the packager.

 This is the location where the final package is put."""

DEFAULT_ZIP_INPUT_DIR = "GameData".format(os.sep)
"""str: default input directory (relative to the root folder) to zip."""

DEFAULT_ZIP_OUTPUT_DIR = "output".format(os.sep)
"""str: default output directory (relative to the root folder) for the packager.

 This is the location where the zipped final package is deposited in."""

DEFAULT_ZIP_FILENAME = "RemoteTech"
"""str: default filename for a zipped file."""

class Packager(object):
    def __init__(self, config_file_path: str, packaging_type: str):
        self.config_file_path = config_file_path
        self.packaging_type = packaging_type
        self.source_dir = None
        self.output_dir = None
        self.cwd = None

        # parse configuration file
        self.config = packager_config.decode(self.config_file_path)

    def setup_cwd(self, relative_path: Optional[str]=None) -> str:
        """Set the current working directory for this script.

        Args:
            relative_path: the current working directory of this script,
                relative to its location.

        Returns: The full path of the current working directory.

        """

        # get current script directory
        script_dir = os.path.dirname(os.path.abspath(__file__))

        # get source directory
        self.source_dir = os.path.abspath(
            os.path.join(script_dir, "..", SOURCE_DIR))

        if not os.path.isdir(self.source_dir):
            raise RuntimeError(
                "Source directory couldn't be found in '{}'.".format(
                    self.source_dir)
            )

        if relative_path:
            # if given path is relative, just add to current directory
            script_dir = os.path.join(script_dir, relative_path)
        else:
            # are we living in '/scripts' dir?
            last_dir = os.path.basename(os.path.normpath(script_dir))
            # are we in 'RemoteTech-Complete/scripts'
            rt_complete_dir = os.path.abspath(os.path.join(script_dir, ".."))
            up_dir = os.path.basename(os.path.normpath(rt_complete_dir)).lower()
            if last_dir != "scripts" or up_dir != "remotetech-complete":
                raise RuntimeError(
                    "This script must reside in 'RemoteTech-Complete/scripts'.")

            # set current working directory to be in 'RemoteTech-Complete/' dir
            script_dir = rt_complete_dir

        os.chdir(script_dir)
        self.cwd = script_dir

        return script_dir

    @staticmethod
    def get_directory(root_dir: Optional[str], path: str,
                      is_relative: bool=True) -> str:
        """Produce a directory path.

        Args:
            root_dir: base path of the directory. Ignored if ìs_relative`is
                False.
            path: relative path to `root_dir` if `is_relative` is True,
                otherwise full path.
            is_relative: if True, then `path` is relative to `root_dir`.
                Otherwise, `root_dir` is ignored and `path` is a full path.

        Returns: The constructed path from the arguments.

        """
        if is_relative:
            directory = os.path.join(root_dir, path)
        else:
            directory = path

        return directory

    def set_output_dir(self, root_dir: Optional[str], output_path: str,
                       is_relative: bool=True) -> str:
        """Construct and check the validity of the output directory for the
        packager.

        Note:
            - If the output directory is not present, it is created.
            - If the output directory is already present, it is wiped.

        See `get_directory` for arguments.

        """
        # construct path
        self.output_dir = Packager.get_directory(root_dir, output_path,
                                                 is_relative)

        # check directory validity
        if not self.output_dir:
            raise RuntimeError("output_dir is None")
        if not os.path.isdir(self.output_dir):
            logging.info("Creating output directory: {}"
                         .format(self.output_dir))

            os.makedirs(self.output_dir, exist_ok=True)
        else:
            logging.info("output dir exists: emptying it")
            # delete directory (and everything it might contain)
            shutil.rmtree(self.output_dir)
            # recreate it
            os.makedirs(self.output_dir, exist_ok=True)

        return self.output_dir

    def handle_modules(self, package_name: str,
                       output_dir: str,
                       modules: Iterable[packager_config.Module]):
        if not modules:
            return

        for module in modules:
            # get package dir, e.g: 'src\\RemoteTech-Transmitter'
            package_dir = self.get_directory(SOURCE_DIR, package_name)
            if not os.path.isdir(package_dir):
                raise RuntimeError(
                    "[-] Error: '{}' is not a directory.".format(package_dir))

            # get binary (assembly) directory, e.g:
            # 'src\\RemoteTech-Transmitter\\src\\RemoteTech-Transmitter\\bin\\release'
            bin_dir = os.path.join(package_dir, package_dir, "bin",
                                   self.packaging_type)
            if not os.path.isdir(bin_dir):
                raise RuntimeError(
                    "[-] Error: '{}' is not a directory.".format(bin_dir))

            # add module name to bin dir to have full path to assembly
            bin_file = os.path.join(bin_dir, module.name)
            if not os.path.isfile(bin_file):
                raise RuntimeError(
                    "[-] Error: '{}' is not file")

            # create a new dir if needed
            if module.dst_dir:
                dst_dir = os.path.join(output_dir, module.dst_dir)
                os.makedirs(dst_dir)
            else:
                dst_dir = output_dir

            # now copy the module to the right place
            dst_file = os.path.join(dst_dir, module.name)
            shutil.copy2(bin_file, dst_file)

    @staticmethod
    def copy_by_pattern(src_dir: str, dst_dir: str, file_list: Iterable[str]):
        for pattern in file_list:
            for file_name in glob.glob(os.path.join(src_dir, pattern)):
                shutil.copy2(file_name, dst_dir)

    def handle_copyable_directories(
            self, package_name: str,
            output_dir: str,
            directories: Iterable[packager_config.Directory]):
        if not directories:
            return

        for directory in directories:
            # get package dir, e.g: 'src\\RemoteTech-Antennas'
            package_dir = self.get_directory(SOURCE_DIR, package_name)
            if not os.path.isdir(package_dir):
                raise RuntimeError(
                    "[-] Error: '{}' is not a directory.".format(package_dir))

            src_dir = os.path.abspath(
                os.path.join(package_dir, directory.src_dir))
            if not os.path.isdir(src_dir):
                RuntimeError("[-] Error: src directory '{}' doesn't exist."
                             .format(src_dir))

            if not directory.dst_dir:
                dst_dir = os.path.abspath(
                    os.path.join(output_dir, directory.src_dir))
            else:
                dst_dir = os.path.join(output_dir, directory.dst_dir)

            # note: we can't use copytree() if the dst dir exists.
            if not os.path.isdir(dst_dir):
                shutil.copytree(
                    src_dir, dst_dir,
                    ignore=shutil.ignore_patterns(*directory.exception_list))
            else:
                self.copy_by_pattern(src_dir, dst_dir, directory.copy_list)

    def package_release(self):
        """Build the release package."""
        for package_entry_name in self.config:
            # get package entry
            package_entry = self.config[package_entry_name]

            # create output directory for package entry
            package_dir_name = package_entry.package_name
            if not package_dir_name:
                package_dir_name = package_entry_name
            package_dir = os.path.join(self.output_dir, package_dir_name)
            os.makedirs(package_dir)

            # handle all compiled modules for this package
            self.handle_modules(package_entry_name, package_dir,
                                package_entry.modules)

            # handle anything we have to copy
            self.handle_copyable_directories(package_entry_name, package_dir,
                                             package_entry.copyable_directories)


def main(args):
    logging.info("Starting packaging script.")

    logging.info("Arguments: {}".format(args))

    packager = Packager(args.config, args.packaging_type)

    # set up directories (cwd, input, output)
    packager.setup_cwd(args.cwd)
    packager.set_output_dir(packager.cwd, args.output_dir,
                            args.output_dir_relative)

    logging.info("Directories:\n\tCurrent working directory: {}"
                 "\n\tOutput directory: {}"
                 .format(packager.cwd, packager.output_dir))

    # do the actual release packaging
    packager.package_release()

    # perform zip if required
    if args.zip:
        logging.info("Zipping {0} as {1}{2}{3}.zip"
                     .format(args.zip_input_dir, 
                             args.zip_output_dir, 
                             os.sep, 
                             DEFAULT_ZIP_FILENAME))

        shutil.make_archive("{0}{1}{2}"
                            .format(args.zip_output_dir, 
                                    os.sep, 
                                    DEFAULT_ZIP_FILENAME), 
                            'zip', 
                            args.zip_input_dir)

    logging.info("Job done. Quitting!")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description='RemoteTech-Complete Packager')

    parser.add_argument(
        "-c", "--config", action="store",
        default=packager_config.DEFAULT_CONFIG_FILE,
        help="Full path to input configuration file."
             " [default: {}]".format(packager_config.DEFAULT_CONFIG_FILE))

    parser.add_argument(
        "--cwd", action="store",
        help="Working directory, relative to this script location.")

    parser.add_argument(
        "-l", "--logging-level", action="store", dest="logging_level",
        help="Logging level (as from logging module).")

    parser.add_argument(
        "-o", "--output-dir", action="store", dest="output_dir",
        default=DEFAULT_OUTPUT_DIR,
        help="Output directory where to put the final package."
             "[default: {}]".format(DEFAULT_OUTPUT_DIR))

    parser.add_argument(
        "--output-dir-relative", action="store_false",
        dest="output_dir_relative", default=True,
        help="Indicates whether the --output-dir is a relative path or not."
             " [default: True]")

    parser.add_argument(
        "-p", "--packaging-type", action="store", dest="packaging_type",
        choices={"release", "debug"}, default="release",
        help="Packaging type: debug or release [default: release].")

    parser.add_argument(
        "--version", action="version", version="%(prog)s {}".format(PACKAGER_VERSION))

    parser.add_argument(
        "-z", "--zip", action="store_true", default=False,
        help="Also zip the package [output in default output directory].")

    parser.add_argument(
        "-zi", "--zip-input-dir", action="store", dest="zip_input_dir",
        default=DEFAULT_ZIP_INPUT_DIR,
        help="Input directory to zip."
             "[default: {}]".format(DEFAULT_ZIP_INPUT_DIR))

    parser.add_argument(
        "-zo", "--zip-output-dir", action="store", dest="zip_output_dir",
        default=DEFAULT_ZIP_OUTPUT_DIR,
        help="Output directory where to place the zipped package."
             "[default: {}]".format(DEFAULT_ZIP_OUTPUT_DIR))

    # parse arguments
    parsed_args = parser.parse_args()

    # setup logging
    logging_level = LOGGING_LEVELS.get(parsed_args.logging_level,
                                       logging.NOTSET)
    logging.basicConfig(level=logging_level)

    # call main
    main(parsed_args)
