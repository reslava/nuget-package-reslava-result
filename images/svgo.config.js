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

    // Custom plugin: add width to every <svg> element.
    // Width is read from the SVGO_WIDTH env var (set by mermaid-to-svg.sh per file):
    //   flowchart TD → SVGO_WIDTH=450   (narrow: architecture / error-propagation diagrams)
    //   flowchart LR → SVGO_WIDTH=900   (wide: pipeline / error-surface diagrams)
    {
      name: 'addWidth900',
      fn: (root, params) => {
        const width = process.env.SVGO_WIDTH || '900';
        // Recursively visit all nodes
        const visit = (node) => {
          if (node.type === 'element' && node.name === 'svg') {
            // Set the width attribute (overwrites any existing one)
            node.attributes.width = width;
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