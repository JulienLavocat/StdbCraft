use spacetimedb::spacetimedb;

#[spacetimedb(table)]
pub struct WorldInfos {
    pub seed: i32,
}

#[spacetimedb(table)]
pub struct Block {
    #[unique]
    #[autoinc]
    pub id: i32,
    pub is_transparent: bool,
    pub top: i32,
    pub bottom: i32,
    pub left: i32,
    pub right: i32,
    pub front: i32,
    pub back: i32,
}

impl Default for Block {
    fn default() -> Self {
        Block {
            id: 0,
            top: -1,
            bottom: -1,
            left: -1,
            right: -1,
            front: -1,
            back: -1,
            is_transparent: false,
        }
    }
}

#[spacetimedb(table)]
#[spacetimedb(index(btree, name = "pos", x, y, z))]
pub struct BlockChange {
    #[primarykey]
    #[autoinc]
    pub id: u64,
    pub x: i32,
    pub y: i32,
    pub z: i32,
    pub block_id: i32,
}
