export function isAllDigits(str: string) {
  const regex = /^\s*\d+(\s*\d+)*\s*$/;
  return regex.test(str);
}

export function removeWhitespaces(str: string) {
  return str.replace(/\s+/g, "");
}

export function thousandSeparator(input: string): string {
  let reversed = input.split("").reverse().join("");

  let chunks = [];
  for (let i = 0; i < reversed.length; i += 3) {
    chunks.push(reversed.substr(i, 3));
  }

  return chunks.join(" ").split("").reverse().join("");
}
