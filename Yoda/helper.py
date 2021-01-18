import logging
import os

logging_format_info = '[%(name)-20s] %(message)s'
logging_format_debug = '[%(name)-20s] %(name)s %(asctime)s - %(levelname)s - %(message)s'
logger = logging.getLogger(os.path.basename(__file__))

def change_logging_file(logging_logfile):
    """
    Configure logging

    info level goes to console. Debug and info level will go to log file if specified with the
    -LOG switch
    :param parser:
    :return:
    """
    logger = logging.getLogger()

    for handler in logger.handlers[:]:
        logger.removeHandler(handler)

    logger.setLevel(logging.INFO)
    formatter = logging.Formatter(logging_format_debug)

    if logging_logfile is not None:
        logger.setLevel(logging.DEBUG)
        fh = logging.FileHandler(logging_logfile)
        fh.setFormatter(formatter)
        logger.addHandler(fh)
    console = logging.StreamHandler()
    console.setLevel(logging.INFO)
    formatter = logging.Formatter(logging_format_info)
    console.setFormatter(formatter)

    logging.getLogger('').addHandler(console)
