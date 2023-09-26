export function isAllDigits(str: string) {
  const regex = /^\s*\d+(\s*\d+)*\s*$/;
  return regex.test(str);
}

export function removeWhitespaces(str: string) {
  return str.replace(/\s+/g, "");
}
