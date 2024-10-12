db = db.getSiblingDB('prodPlace');

collections = db.getCollectionNames()
if (collections.includes('products') === false) {
    print('no products collection found so starting to seed data')

    // Create a collection
    db.createCollection('products');

    // Insert seed data
    db.products.insertMany([
        {
            name: 'Laptop',
            description: 'High-performance laptop with 16GB RAM and 512GB SSD',
            price: 1299.99,
            customFields: JSON.stringify({color: 'Silver', weight: '1.8kg'})
        },
        {
            name: 'Smartphone',
            description: 'Latest model with 5G capability and triple camera setup',
            price: 799.99,
            customFields: JSON.stringify({color: 'Black', storage: '256GB'})
        },
        {
            name: 'Headphones',
            description: 'Noise-cancelling wireless headphones with 30-hour battery life',
            price: 249.99,
            customFields: JSON.stringify({color: 'White', type: 'Over-ear'})
        }
    ]);

    db.createUser(
        {
            user: 'admin',
            pwd: 'adminpassword',
            roles: [{role: 'dbOwner', db: 'prodPlace'}]
        }
    )


    print('Database initialized with seed data.');
}