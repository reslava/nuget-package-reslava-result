module.exports = {
  multipass: true,

  plugins: [
    // Default optimizations, with convertShapeToPath disabled
    {
      name: 'preset-default',
      params: {
        overrides: {
          convertShapeToPath: false,
        },
      },
    },

    // Explicitly turn off removeDimensions – it's not part of preset-default
    {
      name: 'removeDimensions',
      active: false,
    },

    // Custom plugin: add width="900" to every <svg> element
    {
      name: 'addWidth900',
      fn: (root, params) => {
        // Recursively visit all nodes
        const visit = (node) => {
          if (node.type === 'element' && node.name === 'svg') {
            // Set the width attribute (overwrites any existing one)
            node.attributes.width = '900';
            // Optional: preserve aspect ratio by setting height="auto"
            // node.attributes.height = 'auto';
          }
          if (node.children) {
            node.children.forEach(visit);
          }
        };
        visit(root);
        return root;
      },
    },
  ],
};