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

    // Strip background-color from the <svg> element's inline style.
    // Mermaid (newer versions) emits style="...;background-color:#fff" directly on <svg>.
    {
      name: 'removeSvgBackgroundColor',
      fn: () => ({
        element: {
          enter(node) {
            if (node.name !== 'svg' || !node.attributes.style) return;
            node.attributes.style = node.attributes.style
              .split(';')
              .filter((part) => !part.trim().startsWith('background-color'))
              .join(';')
              .replace(/^;+|;+$/g, ''); // trim leading/trailing semicolons
          },
        },
      }),
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
      name: 'addWidth',
      fn: (root) => {
        const width = process.env.SVGO_WIDTH || '900';
        const visit = (node) => {
          if (node.type === 'element' && node.name === 'svg') {
            node.attributes.width = width;
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
