use crate::tables::Block;

pub struct BlockBuilder {
    is_transparent: bool,
    top: i32,
    bottom: i32,
    left: i32,
    right: i32,
    front: i32,
    back: i32,
}

impl BlockBuilder {
    pub fn new() -> Self {
        Self {
            top: -1,
            bottom: -1,
            left: -1,
            right: -1,
            front: -1,
            back: -1,
            is_transparent: false,
        }
    }

    pub fn transparent(&mut self) -> &mut Self {
        self.is_transparent = true;
        self
    }

    pub fn sides(&mut self, texture: i32) -> &mut Self {
        self.left = texture;
        self.right = texture;
        self.front = texture;
        self.back = texture;
        self
    }

    pub fn all(&mut self, texture: i32) -> &mut Self {
        self.sides(texture);
        self.top = texture;
        self.bottom = texture;
        self
    }

    pub fn top(&mut self, texture: i32) -> &mut Self {
        self.top = texture;
        self
    }

    pub fn bottom(&mut self, texture: i32) -> &mut Self {
        self.bottom = texture;
        self
    }

    pub fn build(&self) -> Block {
        Block {
            id: 0, // Id is auto-incremented by SpacetimeDB
            top: self.top,
            bottom: self.bottom,
            left: self.left,
            right: self.right,
            front: self.front,
            back: self.back,
            is_transparent: self.is_transparent,
        }
    }
}