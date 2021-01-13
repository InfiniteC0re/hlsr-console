const fs = require("fs");

const START_OFFSET = 0;

const MAX_EXT_SIZE = 8;
const MAX_GAME_SIZE = 32;
const MAX_TITLE_SIZE = 64;
const MAX_AUTHOR_SIZE = 64;
const MAX_VIDEOFILE_SIZE = 128;
const MAX_DESC_SIZE = 256;

const MAX_HEADER_SIZE = 1024;
const EXT_NAME = "HLSRC10";

function getByteAndASCII(buffer, index) {
  return [buffer[index], String.fromCharCode(buffer[index])];
}

function readOffsetString(buffer, start, end, addStartToEnd) {
  let ret = "";

  if (addStartToEnd) end = start + end;
  for (let i = start; i < end; i++) {
    let [BYTE, ASCII] = getByteAndASCII(buffer, i);

    if (BYTE != 0) ret += ASCII;
  }

  return ret;
}

module.exports = (input) => {
  const buffer = fs.readFileSync(input);

  let FILE_FORMAT = readOffsetString(buffer, START_OFFSET, MAX_EXT_SIZE, true);
  
  if (FILE_FORMAT != EXT_NAME) return null;
  
  try {
    let SCRIPT_TITLE = readOffsetString(
      buffer,
      START_OFFSET + MAX_EXT_SIZE,
      MAX_TITLE_SIZE,
      true
    );
    let SCRIPT_DESCRIPTION = readOffsetString(
      buffer,
      START_OFFSET + MAX_TITLE_SIZE,
      MAX_DESC_SIZE,
      true
    );
    let SCRIPT_GAME = readOffsetString(
      buffer,
      START_OFFSET + MAX_TITLE_SIZE + MAX_DESC_SIZE,
      MAX_GAME_SIZE,
      true
    );
    let SCRIPT_VIDEOFILE = readOffsetString(
      buffer,
      START_OFFSET + MAX_TITLE_SIZE + MAX_DESC_SIZE + MAX_GAME_SIZE,
      MAX_VIDEOFILE_SIZE,
      true
    );
    let SCRIPT_AUTHOR = readOffsetString(
      buffer,
      START_OFFSET +
        MAX_TITLE_SIZE +
        MAX_DESC_SIZE +
        MAX_GAME_SIZE +
        MAX_VIDEOFILE_SIZE,
      MAX_AUTHOR_SIZE,
      true
    );
    
    let SCRIPT_DATA = readOffsetString(buffer, MAX_HEADER_SIZE, buffer.length);
    SCRIPT_DATA = JSON.parse(SCRIPT_DATA);

    return {
      title: SCRIPT_TITLE,
      description: SCRIPT_DESCRIPTION,
      game: SCRIPT_GAME,
      video: SCRIPT_VIDEOFILE,
      author: SCRIPT_AUTHOR,
      data: SCRIPT_DATA
    }
  }catch(e) {
    return null;
  }
}