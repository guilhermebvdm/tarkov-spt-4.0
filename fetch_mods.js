const https = require('https');

const urls = [
  'https://forge.sp-tarkov.com/mod/2706/orbit',
  'https://forge.sp-tarkov.com/mod/2703/manimals-hacker-mod',
  'https://forge.sp-tarkov.com/mod/2667/picture-in-picture-disabler',
  'https://forge.sp-tarkov.com/mod/526/all-the-clothes',
  'https://forge.sp-tarkov.com/mod/2705/hideout-shootout'
];

function fetch(url) {
  return new Promise((resolve, reject) => {
    https.get(url, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => resolve({url, data}));
    }).on('error', reject);
  });
}

Promise.all(urls.map(fetch)).then(results => {
  const mods = [];
  for (const {url, data} of results) {
    const id = url.split('/mod/')[1].split('/')[0];
    const slug = url.split('/mod/')[1].split('/')[1] || '';
    
    const titleMatch = data.match(/<title>(.*?) - Mod Details - The Forge<\/title>/);
    const imgMatch = data.match(/<meta property="og:image" content="https:\/\/forge-static\.sp-tarkov\.com\/mods\/(.*?)"/);
    const repoMatch = data.match(/href="(https:\/\/(?:github|gitlab)\.com\/[^"]*)"/g);
    const verMatch = data.match(/SPT \d+\.\d+(?:\.\d+)?/g);
    const descMatch = data.match(/<meta\s+name="description"\s+content="The details for.*?on The Forge\.\s*(.*?)"/);
    
    mods.push({
      id,
      slug,
      name: titleMatch ? titleMatch[1] : null,
      image: imgMatch ? imgMatch[1] : null,
      repos: repoMatch ? [...new Set(repoMatch.map(r => r.split('"')[1]))] : [],
      version: verMatch ? [...new Set(verMatch)][0] : null,
      description: descMatch ? descMatch[1] : null
    });
  }
  console.log(JSON.stringify(mods, null, 2));
});
