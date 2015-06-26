INSERT INTO category (name, code) VALUES ('中華', 'chinese');
INSERT INTO category (name, code) VALUES ('イタリアン', 'italian');

INSERT INTO shop (name, category_id) VALUES ('天祥', (SELECT id FROM category WHERE code = 'chinese'));
INSERT INTO shop (name, category_id) VALUES ('エスペリア', (SELECT id FROM category WHERE code = 'italian'));